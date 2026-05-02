namespace DeskFlowAI.Models;

public sealed class AuditLogEntry
{
    private AuditLogEntry()
    {
        ActorEmail = string.Empty;
        Action = string.Empty;
        EntityName = string.Empty;
        Details = string.Empty;
    }

    public AuditLogEntry(int id, DateTime occurredAt, string actorEmail, string action, string entityName, string details)
    {
        Id = id;
        OccurredAt = occurredAt;
        ActorEmail = actorEmail;
        Action = action;
        EntityName = entityName;
        Details = details;
    }

    public AuditLogEntry(DateTime occurredAt, string actorEmail, string action, string entityName, string details)
    {
        OccurredAt = occurredAt;
        ActorEmail = actorEmail;
        Action = action;
        EntityName = entityName;
        Details = details;
    }

    public int Id { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public string ActorEmail { get; private set; }

    public string Action { get; private set; }

    public string EntityName { get; private set; }

    public string Details { get; private set; }
}
