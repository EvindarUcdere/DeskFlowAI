using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoOverviewService
{
    public OverviewInsights GetInsights()
    {
        using DeskFlowDbContext dbContext = new();

        List<WorkProject> projects = dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Customer)
            .ToList();
        List<WorkTask> tasks = dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.Project)
            .ThenInclude(project => project!.Customer)
            .Include(task => task.AssignedEmployee)
            .ToList();
        List<ProjectDocument> documents = dbContext.ProjectDocuments
            .AsNoTracking()
            .ToList();
        List<Employee> employees = dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.AssignedTasks)
            .ToList();

        int completedProjects = projects.Count(project => project.Status == ProjectStatusNames.Completed);
        int doneTasks = tasks.Count(task => task.Status == TaskStatusNames.Done);
        int analyzedDocuments = documents.Count(document => document.AIAnalysisStatus == AIAnalysisStatusNames.Analyzed);

        return new OverviewInsights
        {
            ProjectCompletionPercent = Percent(completedProjects, projects.Count),
            ProjectCompletionText = $"{completedProjects}/{projects.Count} projects completed",
            TaskCompletionPercent = Percent(doneTasks, tasks.Count),
            TaskCompletionText = $"{doneTasks}/{tasks.Count} tasks done",
            AIUsagePercent = Percent(analyzedDocuments, documents.Count),
            AIUsageText = $"{analyzedDocuments}/{documents.Count} documents analyzed",
            WorkloadItems = BuildWorkloadItems(employees),
            OverdueHeatmapItems = BuildOverdueHeatmapItems(tasks),
            ProductivityItems = BuildProductivityItems(employees),
            AIMetricItems = BuildAIMetricItems(documents)
        };
    }

    private static List<OverviewWorkloadItem> BuildWorkloadItems(List<Employee> employees)
    {
        int maxOpenTasks = Math.Max(1, employees.Select(employee => employee.OpenTaskCount).DefaultIfEmpty().Max());

        return employees
            .OrderByDescending(employee => employee.OpenTaskCount)
            .ThenBy(employee => employee.FullName)
            .Take(6)
            .Select(employee => new OverviewWorkloadItem(
                employee.FullName,
                $"{employee.RoleTitle} - {employee.Department}",
                employee.AvailabilityStatus,
                employee.OpenTaskCount,
                Percent(employee.OpenTaskCount, maxOpenTasks)))
            .ToList();
    }

    private static List<OverviewHeatmapItem> BuildOverdueHeatmapItems(List<WorkTask> tasks)
    {
        return tasks
            .Where(task => task.IsOverdue)
            .GroupBy(task => task.ProjectId)
            .Select(group =>
            {
                WorkProject? project = group.First().Project;
                int overdueCount = group.Count();
                return new OverviewHeatmapItem(
                    project?.Name ?? "Unknown project",
                    project?.Customer?.CompanyName ?? "Unknown customer",
                    overdueCount,
                    overdueCount >= 3 ? "Danger" : "Warning");
            })
            .OrderByDescending(item => item.OverdueTaskCount)
            .ThenBy(item => item.ProjectName)
            .Take(6)
            .ToList();
    }

    private static List<OverviewProductivityItem> BuildProductivityItems(List<Employee> employees)
    {
        return employees
            .Where(employee => employee.AssignedTasks.Count > 0)
            .Select(employee =>
            {
                int totalTasks = employee.AssignedTasks.Count;
                int doneTasks = employee.AssignedTasks.Count(task => task.Status == TaskStatusNames.Done);
                return new OverviewProductivityItem(
                    employee.FullName,
                    doneTasks,
                    totalTasks,
                    Percent(doneTasks, totalTasks));
            })
            .OrderByDescending(item => item.CompletionPercent)
            .ThenByDescending(item => item.TotalTaskCount)
            .Take(6)
            .ToList();
    }

    private static List<OverviewAIMetricItem> BuildAIMetricItems(List<ProjectDocument> documents)
    {
        int totalDocuments = documents.Count;
        int analyzedDocuments = documents.Count(document => document.AIAnalysisStatus == AIAnalysisStatusNames.Analyzed);
        int readyReviews = documents.Count(document => document.AIReviewStatus == AIReviewStatusNames.Ready);
        int highRiskDocuments = documents.Count(document => document.AIRiskLevel == "High" || document.AIRiskScore >= 75);
        int complianceIssues = documents.Count(document => document.AIComplianceStatus == AIComplianceStatusNames.ViolationDetected);

        return
        [
            new("Analyzed", analyzedDocuments.ToString(), $"{Percent(analyzedDocuments, totalDocuments)}% of documents", "Success"),
            new("Ready review", readyReviews.ToString(), "AI outputs waiting for human review", readyReviews > 0 ? "Warning" : "Info"),
            new("High risk", highRiskDocuments.ToString(), "Risk score >= 75 or High risk level", highRiskDocuments > 0 ? "Danger" : "Success"),
            new("Compliance", complianceIssues.ToString(), "Violation detected documents", complianceIssues > 0 ? "Danger" : "Success")
        ];
    }

    private static int Percent(int part, int total)
    {
        return total <= 0 ? 0 : (int)Math.Round(part * 100.0 / total);
    }
}
