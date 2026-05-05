using DeskFlowAI.Data;
using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DemoDashboardService
{
    public DashboardSummary GetSummaryFor(UserSession user)
    {
        using DeskFlowDbContext dbContext = new();
        int activeProjects = dbContext.Projects.Count(project => project.Status == ProjectStatusNames.Active);
        IQueryable<WorkTask> taskQuery = dbContext.Tasks;

        if (user.Role == RoleNames.Staff && user.EmployeeId.HasValue)
        {
            taskQuery = taskQuery.Where(task => task.AssignedEmployeeId == user.EmployeeId.Value);
            activeProjects = taskQuery
                .Select(task => task.ProjectId)
                .Distinct()
                .Count();
        }

        int openTasks = taskQuery.Count(task => task.Status != TaskStatusNames.Done);
        int overdueTasks = taskQuery.Count(task =>
            task.Status != TaskStatusNames.Done
            && task.DueDate.HasValue
            && task.DueDate.Value.Date < DateTime.Today);
        IQueryable<ProjectDocument> documentQuery = dbContext.ProjectDocuments;

        if (user.Role == RoleNames.Staff && user.EmployeeId.HasValue)
        {
            documentQuery = documentQuery.Where(document =>
                document.Project!.Tasks.Any(task => task.AssignedEmployeeId == user.EmployeeId.Value));
        }

        int pendingAiDocuments = documentQuery.Count(document =>
            document.AIAnalysisStatus != AIAnalysisStatusNames.Analyzed);

        return new DashboardSummary(
            activeProjects: activeProjects,
            openTasks: openTasks,
            overdueTasks: overdueTasks,
            pendingAiDocuments: pendingAiDocuments);
    }
}
