namespace DeskFlowAI.Models;

public sealed class DashboardSummary
{
    public DashboardSummary(
        int activeProjects,
        int openTasks,
        int overdueTasks,
        int pendingAiDocuments,
        int analyzedAiDocuments,
        int internalOnlyDocuments,
        int externalAIAllowedDocuments,
        int needsApprovalDocuments,
        int blockedDocuments)
    {
        ActiveProjects = activeProjects;
        OpenTasks = openTasks;
        OverdueTasks = overdueTasks;
        PendingAiDocuments = pendingAiDocuments;
        AnalyzedAiDocuments = analyzedAiDocuments;
        InternalOnlyDocuments = internalOnlyDocuments;
        ExternalAIAllowedDocuments = externalAIAllowedDocuments;
        NeedsApprovalDocuments = needsApprovalDocuments;
        BlockedDocuments = blockedDocuments;
    }

    public int ActiveProjects { get; }

    public int OpenTasks { get; }

    public int OverdueTasks { get; }

    public int PendingAiDocuments { get; }

    public int AnalyzedAiDocuments { get; }

    public int InternalOnlyDocuments { get; }

    public int ExternalAIAllowedDocuments { get; }

    public int NeedsApprovalDocuments { get; }

    public int BlockedDocuments { get; }
}
