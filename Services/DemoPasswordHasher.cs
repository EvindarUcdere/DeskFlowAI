using System.Security.Cryptography;
using System.Text;

namespace DeskFlowAI.Services;

public static class DemoPasswordHasher
{
    public static string Hash(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string passwordHash)
    {
        return Hash(password).Equals(passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}
