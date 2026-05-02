using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoDashboardService
{
    public DashboardSummary GetSummaryFor(UserSession user)
    {
        if (user.Role == "Admin")
        {
            return new DashboardSummary(
                activeProjects: 12,
                openTasks: 38,
                overdueTasks: 5,
                pendingAiDocuments: 9);
        }

        return new DashboardSummary(
            activeProjects: 4,
            openTasks: 11,
            overdueTasks: 1,
            pendingAiDocuments: 2);
    }
}
