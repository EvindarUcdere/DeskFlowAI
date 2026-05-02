using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoAuditLogService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public DemoAuditLogService()
    {
        _dbContext.Database.EnsureCreated();
    }

    public List<AuditLogEntry> GetEntries()
    {
        return _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(entry => entry.OccurredAt)
            .ToList();
    }

    public AuditLogEntry CreateEntry(UserSession actor, string action, string entityName, string details)
    {
        AuditLogEntry entry = new(
            DateTime.Now,
            actor.Email,
            action,
            entityName,
            details);

        _dbContext.AuditLogs.Add(entry);
        _dbContext.SaveChanges();

        return entry;
    }
}
