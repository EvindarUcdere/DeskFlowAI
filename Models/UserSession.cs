namespace DeskFlowAI.Models;

public sealed class UserSession
{
    public UserSession(string fullName, string email, string role)
    {
        FullName = fullName;
        Email = email;
        Role = role;
    }

    public string FullName { get; }

    public string Email { get; }

    public string Role { get; }
}
