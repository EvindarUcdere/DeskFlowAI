namespace DeskFlowAI.Models;

public sealed class UserNotification
{
    private UserNotification()
    {
        RecipientEmail = string.Empty;
        Title = string.Empty;
        Message = string.Empty;
        Severity = string.Empty;
        CreatedByEmail = string.Empty;
    }

    public UserNotification(
        string recipientEmail,
        string title,
        string message,
        string severity,
        string createdByEmail,
        int? relatedProjectId)
    {
        RecipientEmail = recipientEmail;
        Title = title;
        Message = message;
        Severity = severity;
        CreatedByEmail = createdByEmail;
        RelatedProjectId = relatedProjectId;
        CreatedAt = DateTime.Now;
        IsRead = false;
    }

    public int Id { get; private set; }

    public string RecipientEmail { get; private set; }

    public string Title { get; private set; }

    public string Message { get; private set; }

    public string Severity { get; private set; }

    public string CreatedByEmail { get; private set; }

    public int? RelatedProjectId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTime.Now;
    }
}
