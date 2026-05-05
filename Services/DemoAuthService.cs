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
            .SingleOrDefault(user => user.Email == normalizedEmail);

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
            GetPermissionsForRole(user.Role));
    }

    private static IReadOnlyCollection<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            RoleNames.Admin =>
                [
                    PermissionNames.CustomerCreate,
                    PermissionNames.CustomerUpdate,
                    PermissionNames.CustomerDelete,
                    PermissionNames.ProjectCreate,
                    PermissionNames.ProjectUpdate,
                    PermissionNames.TaskCreate,
                    PermissionNames.TaskUpdate,
                    PermissionNames.EmployeeManage,
                    PermissionNames.UserManage,
                    PermissionNames.DocumentCreate,
                    PermissionNames.DocumentUpdate
                ],

            RoleNames.Manager =>
                [
                    PermissionNames.CustomerCreate,
                    PermissionNames.CustomerUpdate,
                    PermissionNames.ProjectCreate,
                    PermissionNames.ProjectUpdate,
                    PermissionNames.TaskCreate,
                    PermissionNames.TaskUpdate,
                    PermissionNames.EmployeeManage,
                    PermissionNames.DocumentCreate,
                    PermissionNames.DocumentUpdate
                ],

            RoleNames.Staff =>
                [
                    PermissionNames.TaskCreate,
                    PermissionNames.TaskUpdate
                ],

            _ => []
        };
    }
}
