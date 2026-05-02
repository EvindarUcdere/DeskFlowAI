using DeskFlowAI.Data;
using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoDashboardService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public DashboardSummary GetSummaryFor(UserSession user)
    {
        int activeProjects = _dbContext.Projects.Count(project => project.Status == ProjectStatusNames.Active);

        return new DashboardSummary(
            activeProjects: activeProjects,
            openTasks: 11,
            overdueTasks: 1,
            pendingAiDocuments: 2);
    }
}
