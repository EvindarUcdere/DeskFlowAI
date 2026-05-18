using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoProjectCommunicationService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public int CreateProjectNoteAndNotifyTeam(int projectId, string message, UserSession author)
    {
        ProjectNote note = new(projectId, message, author.Email);
        _dbContext.ProjectNotes.Add(note);

        List<string> recipientEmails = _dbContext.UserAccounts
            .AsNoTracking()
            .Where(user => user.IsActive
                && user.EmployeeId.HasValue
                && user.Email != author.Email
                && _dbContext.Tasks.Any(task => task.ProjectId == projectId
                    && task.AssignedEmployeeId == user.EmployeeId.Value))
            .Select(user => user.Email)
            .Distinct()
            .ToList();

        WorkProject? project = _dbContext.Projects
            .AsNoTracking()
            .SingleOrDefault(project => project.Id == projectId);
        string projectName = project?.Name ?? "Project";

        foreach (string recipientEmail in recipientEmails)
        {
            _dbContext.UserNotifications.Add(new UserNotification(
                recipientEmail,
                "Project note",
                $"{projectName}: {message}",
                "Info",
                author.Email,
                projectId));
        }

        _dbContext.SaveChanges();

        return recipientEmails.Count;
    }

    public List<UserNotification> GetUnreadNotificationsFor(string recipientEmail)
    {
        return _dbContext.UserNotifications
            .AsNoTracking()
            .Where(notification => notification.RecipientEmail == recipientEmail && !notification.IsRead)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(6)
            .ToList();
    }
}
