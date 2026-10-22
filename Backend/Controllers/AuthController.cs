using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Helpers;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BeeMartDbContext _context;

    public AuthController(BeeMartDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<object>.Fail("Username dan Password wajib diisi."));
        }

        var user = await _context.MsUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Stsrc);

        if (user == null)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid login"));
        }

        // Validate Role (case-insensitive)
        if (!string.Equals(user.Role, request.Role, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid login"));
        }

        // Validate Password Hash
        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid login"));
        }

        var session = new UserSessionDto
        {
            IdUser = user.IdUser,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        };

        return Ok(ApiResponse<UserSessionDto>.Ok(session, "Login berhasil"));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.RePassword))
        {
            return BadRequest(ApiResponse<object>.Fail("Username, Full Name, Password & Re-Password harus diisi."));
        }

        if (request.Password != request.RePassword)
        {
            return BadRequest(ApiResponse<object>.Fail("Password dan Re-Password tidak cocok."));
        }

        // Check if username already exists
        bool userExists = await _context.MsUsers
            .AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (userExists)
        {
            return BadRequest(ApiResponse<object>.Fail("username has already exists"));
        }

        // Generate unique 5-digit IdUser
        string newIdUser;
        var random = new Random();
        do
        {
            newIdUser = random.Next(10000, 99999).ToString();
        } while (await _context.MsUsers.AnyAsync(u => u.IdUser == newIdUser));

        string passwordHash = PasswordHelper.HashPassword(request.Password);

        var newUser = new MsUser
        {
            IdUser = newIdUser,
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = passwordHash,
            Role = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        };

        _context.MsUsers.Add(newUser);
        await _context.SaveChangesAsync();

        var session = new UserSessionDto
        {
            IdUser = newUser.IdUser,
            Username = newUser.Username,
            FullName = newUser.FullName,
            Role = newUser.Role
        };

        return Ok(ApiResponse<UserSessionDto>.Ok(session, "Registrasi berhasil"));
    }
}
