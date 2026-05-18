namespace DeskFlowAI.Models;

public sealed class DashboardNotification
{
    public DashboardNotification(string title, string message, string severity)
    {
        Title = title;
        Message = message;
        Severity = severity;
    }

    public string Title { get; }

    public string Message { get; }

    public string Severity { get; }
}
