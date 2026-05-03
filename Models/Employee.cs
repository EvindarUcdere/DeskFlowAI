namespace DeskFlowAI.Models;

public sealed class Employee
{
    private Employee()
    {
        FullName = string.Empty;
        Email = string.Empty;
        Department = string.Empty;
        RoleTitle = string.Empty;
        AvailabilityStatus = string.Empty;
        Skills = string.Empty;
        BackupEmployeeName = string.Empty;
    }

    public Employee(
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availabilityStatus,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        FullName = fullName;
        Email = email;
        Department = department;
        RoleTitle = roleTitle;
        AvailabilityStatus = availabilityStatus;
        LeaveStart = leaveStart;
        LeaveEnd = leaveEnd;
        Skills = skills;
        BackupEmployeeName = backupEmployeeName;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; private set; }

    public string FullName { get; private set; }

    public string Email { get; private set; }

    public string Department { get; private set; }

    public string RoleTitle { get; private set; }

    public string AvailabilityStatus { get; private set; }

    public DateTime? LeaveStart { get; private set; }

    public DateTime? LeaveEnd { get; private set; }

    public string Skills { get; private set; }

    public string BackupEmployeeName { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public List<WorkTask> AssignedTasks { get; private set; } = [];

    public int OpenTaskCount => AssignedTasks.Count(task => task.Status != TaskStatusNames.Done);

    public DateTime? NextOpenTaskDueDate => AssignedTasks
        .Where(task => task.Status != TaskStatusNames.Done && task.DueDate.HasValue)
        .OrderBy(task => task.DueDate)
        .Select(task => task.DueDate)
        .FirstOrDefault();

    public bool NeedsCoverage => AvailabilityStatus is EmployeeAvailabilityNames.OnLeave
        or EmployeeAvailabilityNames.EmergencyCover;

    public void UpdateDetails(
        string fullName,
        string email,
        string department,
        string roleTitle,
        string availabilityStatus,
        DateTime? leaveStart,
        DateTime? leaveEnd,
        string skills,
        string backupEmployeeName)
    {
        FullName = fullName;
        Email = email;
        Department = department;
        RoleTitle = roleTitle;
        AvailabilityStatus = availabilityStatus;
        LeaveStart = leaveStart;
        LeaveEnd = leaveEnd;
        Skills = skills;
        BackupEmployeeName = backupEmployeeName;
    }
}
