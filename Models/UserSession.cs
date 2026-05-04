namespace DeskFlowAI.Models;

public sealed class UserSession
{
    public UserSession(string fullName, string email, string role, int? employeeId, IReadOnlyCollection<string> permissions)
    {
        FullName = fullName;
        Email = email;
        Role = role;
        EmployeeId = employeeId;
        Permissions = permissions;
    }

    public string FullName { get; }

    public string Email { get; }

    public string Role { get; }

    public int? EmployeeId { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }
}
