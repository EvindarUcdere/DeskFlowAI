namespace DeskFlowAI.Models;

public sealed class ProjectTeamMemberSummary
{
    public ProjectTeamMemberSummary(
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availabilityStatus,
        int taskCount,
        int openTaskCount)
    {
        FullName = fullName;
        Email = email;
        Department = department;
        RoleTitle = roleTitle;
        AvailabilityStatus = availabilityStatus;
        TaskCount = taskCount;
        OpenTaskCount = openTaskCount;
    }

    public string FullName { get; }

    public string Email { get; }

    public string Department { get; }

    public string RoleTitle { get; }

    public string AvailabilityStatus { get; }

    public int TaskCount { get; }

    public int OpenTaskCount { get; }

    public string WorkloadText => $"{OpenTaskCount}/{TaskCount} open";

    public string RoleText => $"{RoleTitle} - {Department}";
}
