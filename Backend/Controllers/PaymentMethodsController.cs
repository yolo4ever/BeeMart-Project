using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/v1/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly BeeMartDbContext _context;

    public PaymentMethodsController(BeeMartDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var methods = await _context.LtPaymentMethods
            .Where(m => m.Stsrc)
            .OrderBy(m => m.IdPaymentMethod)
            .Select(m => new PaymentMethodDto
            {
                IdPaymentMethod = m.IdPaymentMethod,
                MethodName = m.MethodName
            })
            .ToListAsync();

        return Ok(ApiResponse<List<PaymentMethodDto>>.Ok(methods));
    }
}
