namespace DeskFlowAI.Models;

public sealed class UserAccount
{
    private UserAccount()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public UserAccount(string email, string passwordHash, string role, int? employeeId, bool isActive)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        EmployeeId = employeeId;
        IsActive = isActive;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string Role { get; private set; }

    public int? EmployeeId { get; private set; }

    public Employee? Employee { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string EmployeeName => Employee?.FullName ?? "Unlinked";

    public void UpdateDetails(string email, string role, int? employeeId, bool isActive)
    {
        Email = email;
        Role = role;
        EmployeeId = employeeId;
        IsActive = isActive;
    }

    public void ResetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
