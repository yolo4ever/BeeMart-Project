using System.Globalization;
using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Helpers;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BeeMartDbContext _context;

    public TransactionsController(BeeMartDbContext context)
    {
        _context = context;
    }

    [HttpGet("next-invoice")]
    public async Task<IActionResult> GetNextInvoice([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(ApiResponse<object>.Fail("User ID wajib diisi."));
        }

        string nextInvoice = await InvoiceNumberHelper.GenerateNextInvoiceNumberAsync(_context, userId);
        return Ok(ApiResponse<NextInvoiceDto>.Ok(new NextInvoiceDto { InvoiceNo = nextInvoice }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Keranjang belanja tidak boleh kosong."));
        }

        var user = await _context.MsUsers.FirstOrDefaultAsync(u => u.IdUser == request.IdUser && u.Stsrc);
        if (user == null)
        {
            return BadRequest(ApiResponse<object>.Fail("Pengguna tidak ditemukan."));
        }

        var paymentMethod = await _context.LtPaymentMethods.FirstOrDefaultAsync(m => m.IdPaymentMethod == request.IdPaymentMethod && m.Stsrc);
        if (paymentMethod == null)
        {
            return BadRequest(ApiResponse<object>.Fail("Metode pembayaran tidak valid."));
        }

        // Fetch products and validate stock
        var productIds = request.Items.Select(i => i.IdProduct).Distinct().ToList();
        var products = await _context.MsProducts
            .Where(p => productIds.Contains(p.IdProduct) && p.Stsrc)
            .ToDictionaryAsync(p => p.IdProduct);

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.IdProduct, out var prod))
            {
                return BadRequest(ApiResponse<object>.Fail("Produk tidak ditemukan atau tidak aktif."));
            }

            if (item.Qty <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail($"Quantity untuk {prod.Name} harus lebih dari 0."));
            }

            if (item.Qty > prod.Stock)
            {
                return BadRequest(ApiResponse<object>.Fail("Jumlah barang yang dimasukkan melebihi sisa stok."));
            }
        }

        // Generate / verify invoice number
        string invoiceNo = request.InvoiceNo;
        if (string.IsNullOrWhiteSpace(invoiceNo))
        {
            invoiceNo = await InvoiceNumberHelper.GenerateNextInvoiceNumberAsync(_context, request.IdUser);
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            string transactionId = Guid.NewGuid().ToString();
            DateTime now = DateTime.Now;

            decimal calculatedTotalPrice = 0;
            var details = new List<TrTransactionDetail>();

            foreach (var item in request.Items)
            {
                var prod = products[item.IdProduct];
                decimal price = prod.Price;
                decimal discount = prod.Discount;
                decimal discountedUnitPrice = price - (price * (discount / 100m));
                decimal subTotal = discountedUnitPrice * item.Qty;

                calculatedTotalPrice += subTotal;

                // Deduct stock
                prod.Stock -= item.Qty;
                prod.Dateup = now;
                prod.Userup = user.Username;

                details.Add(new TrTransactionDetail
                {
                    IdTransactionDetail = Guid.NewGuid().ToString(),
                    IdTransaction = transactionId,
                    IdProduct = prod.IdProduct,
                    Qty = item.Qty,
                    Price = price,
                    Discount = discount,
                    SubTotal = subTotal,
                    Stsrc = true,
                    Datein = now,
                    Userin = user.Username
                });
            }

            var transaction = new TrTransaction
            {
                IdTransaction = transactionId,
                InvoiceNo = invoiceNo,
                TransactionDate = now,
                IdUser = user.IdUser,
                IdPaymentMethod = paymentMethod.IdPaymentMethod,
                TotalPrice = calculatedTotalPrice,
                Stsrc = true,
                Datein = now,
                Userin = user.Username,
                TransactionDetails = details
            };

            _context.TrTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            // Next invoice number for next transaction
            string nextInvoice = await InvoiceNumberHelper.GenerateNextInvoiceNumberAsync(_context, user.IdUser);

            return Ok(ApiResponse<object>.Ok(new
            {
                invoiceNo = transaction.InvoiceNo,
                totalPrice = transaction.TotalPrice,
                nextInvoiceNo = nextInvoice
            }, "Transaksi berhasil disimpan."));
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            return StatusCode(500, ApiResponse<object>.Fail($"Terjadi kesalahan saat memproses transaksi: {ex.Message}"));
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? date,
        [FromQuery] string? role,
        [FromQuery] string? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return BadRequest(ApiResponse<object>.Fail("Tanggal transaksi wajib dipilih terlebih dahulu."));
        }

        // Parse date - support multiple common formats: yyyy-MM-dd, dd/MM/yyyy, MM/dd/yyyy
        DateTime targetDate;
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "yyyy/MM/dd" };
        if (!DateTime.TryParseExact(date.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
        {
            if (!DateTime.TryParse(date, out targetDate))
            {
                return BadRequest(ApiResponse<object>.Fail("Format tanggal tidak valid. Gunakan format YYYY-MM-DD atau DD/MM/YYYY."));
            }
        }

        DateTime startOfDay = targetDate.Date;
        DateTime endOfDay = startOfDay.AddDays(1);

        var query = _context.TrTransactions
            .Include(t => t.User)
            .Include(t => t.PaymentMethod)
            .Include(t => t.TransactionDetails)
            .Where(t => t.Stsrc && t.TransactionDate >= startOfDay && t.TransactionDate < endOfDay);

        // Role-based filtering
        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            // Cashier role can only view their own transactions
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(ApiResponse<object>.Fail("User ID kasir wajib disertakan."));
            }
            query = query.Where(t => t.IdUser == userId);
        }

        // Must be ordered by TransactionDate ascending
        query = query.OrderBy(t => t.TransactionDate);

        int totalCount = await query.CountAsync();

        if (pageSize <= 0 || pageSize > 20)
        {
            pageSize = 20; // Requirement: maximum 20 data per page
        }

        if (page <= 0) page = 1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionHistoryItemDto
            {
                IdTransaction = t.IdTransaction,
                InvoiceNo = t.InvoiceNo,
                TransactionDate = t.TransactionDate,
                IdUser = t.IdUser,
                CashierUsername = t.User != null ? t.User.Username : "",
                CashierFullName = t.User != null ? t.User.FullName : "",
                TotalItems = t.TransactionDetails.Sum(d => d.Qty),
                PaymentMethodName = t.PaymentMethod != null ? t.PaymentMethod.MethodName : "",
                TotalPrice = t.TotalPrice
            })
            .ToListAsync();

        var result = new PagedResultDto<TransactionHistoryItemDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<PagedResultDto<TransactionHistoryItemDto>>.Ok(result));
    }
}
