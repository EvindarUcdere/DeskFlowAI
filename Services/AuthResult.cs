using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class AuthResult
{
    private AuthResult(bool isSuccess, UserSession? user, string? errorMessage)
    {
        IsSuccess = isSuccess;
        User = user;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public UserSession? User { get; }

    public string? ErrorMessage { get; }

    public static AuthResult Success(UserSession user)
    {
        return new AuthResult(true, user, null);
    }

    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult(false, null, errorMessage);
    }
}
