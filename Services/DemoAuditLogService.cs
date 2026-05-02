using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoAuditLogService
{
    private int _nextId = 1;

    public AuditLogEntry CreateEntry(UserSession actor, string action, string entityName, string details)
    {
        AuditLogEntry entry = new(
            _nextId,
            DateTime.Now,
            actor.Email,
            action,
            entityName,
            details);

        _nextId++;
        return entry;
    }
}
