using DeskFlowAI.Data;
using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoDashboardService
{
    public DashboardSummary GetSummaryFor(UserSession user)
    {
        using DeskFlowDbContext dbContext = new();
        int activeProjects = dbContext.Projects.Count(project => project.Status == ProjectStatusNames.Active);
        int openTasks = dbContext.Tasks.Count(task => task.Status != TaskStatusNames.Done);
        int overdueTasks = dbContext.Tasks.Count(task =>
            task.Status != TaskStatusNames.Done
            && task.DueDate.HasValue
            && task.DueDate.Value.Date < DateTime.Today);

        return new DashboardSummary(
            activeProjects: activeProjects,
            openTasks: openTasks,
            overdueTasks: overdueTasks,
            pendingAiDocuments: 2);
    }
}
