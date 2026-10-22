using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly BeeMartDbContext _context;

    public ProductsController(BeeMartDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Ok(ApiResponse<List<ProductDto>>.Ok(new List<ProductDto>()));
        }

        string trimmedKeyword = keyword.Trim();

        // Search maximum 10 products matching the keyword
        var products = await _context.MsProducts
            .Where(p => p.Stsrc && EF.Functions.Like(p.Name, $"%{trimmedKeyword}%"))
            .OrderBy(p => p.Name)
            .Take(10)
            .Select(p => new ProductDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Weight = p.Weight,
                Stock = p.Stock,
                Price = p.Price,
                Discount = p.Discount
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ProductDto>>.Ok(products));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _context.MsProducts
            .Where(p => p.IdProduct == id && p.Stsrc)
            .Select(p => new ProductDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Weight = p.Weight,
                Stock = p.Stock,
                Price = p.Price,
                Discount = p.Discount
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(ApiResponse<object>.Fail("Barang tidak ditemukan"));
        }

        return Ok(ApiResponse<ProductDto>.Ok(product));
    }
}
