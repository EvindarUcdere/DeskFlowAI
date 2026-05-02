namespace DeskFlowAI.Models;

public sealed class DashboardSummary
{
    public DashboardSummary(int activeProjects, int openTasks, int overdueTasks, int pendingAiDocuments)
    {
        ActiveProjects = activeProjects;
        OpenTasks = openTasks;
        OverdueTasks = overdueTasks;
        PendingAiDocuments = pendingAiDocuments;
    }

    public int ActiveProjects { get; }

    public int OpenTasks { get; }

    public int OverdueTasks { get; }

    public int PendingAiDocuments { get; }
}
