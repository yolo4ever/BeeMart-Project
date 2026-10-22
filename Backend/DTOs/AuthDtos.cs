using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    public string Role { get; set; } = string.Empty; // "Admin" or "Cashier"
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name wajib diisi.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Re-Password wajib diisi.")]
    public string RePassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    public string Role { get; set; } = "Cashier";
}

public class UserSessionDto
{
    public string IdUser { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "") =>
        new ApiResponse<T> { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new ApiResponse<T> { Success = false, Message = message, Errors = errors };
}
