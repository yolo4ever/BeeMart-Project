using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class CreateTransactionItemDto
{
    [Required]
    public string IdProduct { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Qty minimal 1.")]
    public int Qty { get; set; }
}

public class CreateTransactionDto
{
    [Required]
    public string InvoiceNo { get; set; } = string.Empty;

    [Required]
    public string IdUser { get; set; } = string.Empty;

    [Required]
    public int IdPaymentMethod { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Keranjang belanja tidak boleh kosong.")]
    public List<CreateTransactionItemDto> Items { get; set; } = new();
}

public class TransactionHistoryItemDto
{
    public string IdTransaction { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string IdUser { get; set; } = string.Empty;
    public string CashierUsername { get; set; } = string.Empty;
    public string CashierFullName { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}

public class NextInvoiceDto
{
    public string InvoiceNo { get; set; } = string.Empty;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
