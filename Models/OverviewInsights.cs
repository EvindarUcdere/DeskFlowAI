namespace DeskFlowAI.Models;

public sealed class OverviewInsights
{
    public int ProjectCompletionPercent { get; init; }

    public string ProjectCompletionText { get; init; } = string.Empty;

    public int TaskCompletionPercent { get; init; }

    public string TaskCompletionText { get; init; } = string.Empty;

    public int AIUsagePercent { get; init; }

    public string AIUsageText { get; init; } = string.Empty;

    public List<OverviewWorkloadItem> WorkloadItems { get; init; } = [];

    public List<OverviewHeatmapItem> OverdueHeatmapItems { get; init; } = [];

    public List<OverviewProductivityItem> ProductivityItems { get; init; } = [];

    public List<OverviewAIMetricItem> AIMetricItems { get; init; } = [];
}

public sealed class OverviewWorkloadItem
{
    public OverviewWorkloadItem(string name, string roleText, string availabilityStatus, int openTaskCount, int workloadPercent)
    {
        Name = name;
        RoleText = roleText;
        AvailabilityStatus = availabilityStatus;
        OpenTaskCount = openTaskCount;
        WorkloadPercent = workloadPercent;
    }

    public string Name { get; }

    public string RoleText { get; }

    public string AvailabilityStatus { get; }

    public int OpenTaskCount { get; }

    public int WorkloadPercent { get; }

    public string OpenTaskText => $"{OpenTaskCount} open";
}

public sealed class OverviewHeatmapItem
{
    public OverviewHeatmapItem(string projectName, string customerName, int overdueTaskCount, string severity)
    {
        ProjectName = projectName;
        CustomerName = customerName;
        OverdueTaskCount = overdueTaskCount;
        Severity = severity;
    }

    public string ProjectName { get; }

    public string CustomerName { get; }

    public int OverdueTaskCount { get; }

    public string Severity { get; }

    public string OverdueText => $"{OverdueTaskCount} overdue";
}

public sealed class OverviewProductivityItem
{
    public OverviewProductivityItem(string name, int doneTaskCount, int totalTaskCount, int completionPercent)
    {
        Name = name;
        DoneTaskCount = doneTaskCount;
        TotalTaskCount = totalTaskCount;
        CompletionPercent = completionPercent;
    }

    public string Name { get; }

    public int DoneTaskCount { get; }

    public int TotalTaskCount { get; }

    public int CompletionPercent { get; }

    public string CompletionText => $"{DoneTaskCount}/{TotalTaskCount} done";
}

public sealed class OverviewAIMetricItem
{
    public OverviewAIMetricItem(string title, string value, string detail, string severity)
    {
        Title = title;
        Value = value;
        Detail = detail;
        Severity = severity;
    }

    public string Title { get; }

    public string Value { get; }

    public string Detail { get; }

    public string Severity { get; }
}
