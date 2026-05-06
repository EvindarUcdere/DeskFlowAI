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
        int internalOnlyDocuments = documentQuery.Count(document =>
            document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.InternalOnly);
        int externalAIAllowedDocuments = documentQuery.Count(document =>
            document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.ExternalAIAllowed);
        int needsApprovalDocuments = documentQuery.Count(document =>
            document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval);
        int blockedDocuments = documentQuery.Count(document =>
            document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.Blocked);

        return new DashboardSummary(
            activeProjects: activeProjects,
            openTasks: openTasks,
            overdueTasks: overdueTasks,
            pendingAiDocuments: pendingAiDocuments,
            internalOnlyDocuments: internalOnlyDocuments,
            externalAIAllowedDocuments: externalAIAllowedDocuments,
            needsApprovalDocuments: needsApprovalDocuments,
            blockedDocuments: blockedDocuments);
    }
}
