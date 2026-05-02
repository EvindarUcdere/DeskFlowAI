using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoAuthService
{
    private const string DemoEmail = "admin@deskflow.ai";
    private const string DemoPassword = "Admin123";

    public AuthResult SignIn(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failure("Email ve sifre zorunludur.");
        }

        bool isDemoUser = email.Trim().Equals(DemoEmail, StringComparison.OrdinalIgnoreCase)
            && password == DemoPassword;

        if (!isDemoUser)
        {
            return AuthResult.Failure("Email veya sifre hatali. Demo kullanici: admin@deskflow.ai / Admin123");
        }

        UserSession user = new("Evin D.", DemoEmail, "Admin");
        return AuthResult.Success(user);
    }
}
