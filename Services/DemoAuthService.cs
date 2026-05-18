using DeskFlowAI.Models;
using DeskFlowAI.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoAuthService
{
    public AuthResult SignIn(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failure("Email ve sifre zorunludur.");
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        using DeskFlowDbContext dbContext = new();
        UserAccount? user = dbContext.UserAccounts
            .AsNoTracking()
            .Include(user => user.Employee)
            .FirstOrDefault(user => user.Email == normalizedEmail);

        if (user is null
            || !user.IsActive
            || !DemoPasswordHasher.Verify(password, user.PasswordHash))
        {
            return AuthResult.Failure("Email veya sifre hatali. Demo: admin, manager veya staff @deskflow.ai / Admin123");
        }

        return AuthResult.Success(CreateSession(user));
    }

    private static UserSession CreateSession(UserAccount user)
    {
        string fullName = user.Employee?.FullName ?? user.Email;

        return new UserSession(
            fullName,
            user.Email,
            user.Role,
            user.EmployeeId,
            new PermissionPolicyService().GetPermissionsForRole(user.Role));
    }
}
