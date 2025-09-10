using System.Security.Cryptography;
using System.Text;

namespace eCommerce.Core.Helpers;

public class CreatePassword
{
    public static string CreatePasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // hex string
    }

    public static bool VerifyPasswordHash(string password, string storedHash)
    {
        var hashOfInput = CreatePasswordHash(password);
        return hashOfInput == storedHash;
    }
}