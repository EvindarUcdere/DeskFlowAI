namespace DeskFlowAI.Models;

public sealed class ProjectTimelineEntry
{
    public ProjectTimelineEntry(string timeText, string actorEmail, string action, string entityName, string details)
    {
        TimeText = timeText;
        ActorEmail = actorEmail;
        Action = action;
        EntityName = entityName;
        Details = details;
    }

    public string TimeText { get; }

    public string ActorEmail { get; }

    public string Action { get; }

    public string EntityName { get; }

    public string Details { get; }
}
