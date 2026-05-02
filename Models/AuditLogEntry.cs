namespace DeskFlowAI.Models;

public sealed class AuditLogEntry
{
    public AuditLogEntry(int id, DateTime occurredAt, string actorEmail, string action, string entityName, string details)
    {
        Id = id;
        OccurredAt = occurredAt;
        ActorEmail = actorEmail;
        Action = action;
        EntityName = entityName;
        Details = details;
    }

    public int Id { get; }

    public DateTime OccurredAt { get; }

    public string ActorEmail { get; }

    public string Action { get; }

    public string EntityName { get; }

    public string Details { get; }
}
