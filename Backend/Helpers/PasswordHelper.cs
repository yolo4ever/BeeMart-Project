using System.Security.Cryptography;
using System.Text;

namespace BeeMart.Api.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        string hashOfInput = HashPassword(password);
        return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
