using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoAuthService
{
    private const string DemoPassword = "Admin123";

    public AuthResult SignIn(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failure("Email ve sifre zorunludur.");
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();
        bool isKnownUser = normalizedEmail is "admin@deskflow.ai" or "manager@deskflow.ai" or "staff@deskflow.ai";

        if (!isKnownUser || password != DemoPassword)
        {
            return AuthResult.Failure("Email veya sifre hatali. Demo: admin, manager veya staff @deskflow.ai / Admin123");
        }

        return AuthResult.Success(CreateDemoUser(normalizedEmail));
    }

    private static UserSession CreateDemoUser(string email)
    {
        return email switch
        {
            "admin@deskflow.ai" => new UserSession(
                "Evin D.",
                email,
                "Admin",
                [PermissionNames.CustomerCreate, PermissionNames.CustomerUpdate, PermissionNames.CustomerDelete]),

            "manager@deskflow.ai" => new UserSession(
                "Merve A.",
                email,
                "Manager",
                [PermissionNames.CustomerCreate, PermissionNames.CustomerUpdate]),

            _ => new UserSession(
                "Can K.",
                email,
                "Staff",
                [])
        };
    }
}
