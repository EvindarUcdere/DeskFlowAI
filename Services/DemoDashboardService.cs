using DeskFlowAI.Data;
using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoDashboardService
{
    public DashboardSummary GetSummaryFor(UserSession user)
    {
        using DeskFlowDbContext dbContext = new();
        int activeProjects = dbContext.Projects.Count(project => project.Status == ProjectStatusNames.Active);

        return new DashboardSummary(
            activeProjects: activeProjects,
            openTasks: 11,
            overdueTasks: 1,
            pendingAiDocuments: 2);
    }
}
