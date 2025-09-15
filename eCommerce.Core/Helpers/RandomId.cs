using System;
using System.Security.Cryptography;
using System.Text;

public static class IdHelper
{
    public static string GenerateGuidId()
    {
        return Guid.NewGuid().ToString("N"); // N => 32 karakter, '-' yok
    }

    public static string GenerateRandomAlphaNumeric(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        return result.ToString();
    }

    public static string GenerateTimestampId()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string randomPart = GenerateRandomAlphaNumeric(6);
        return $"{timestamp}-{randomPart}";
    }
}