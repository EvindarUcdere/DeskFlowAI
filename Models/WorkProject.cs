namespace DeskFlowAI.Models;

public sealed class WorkProject
{
    private WorkProject()
    {
        Name = string.Empty;
        Status = string.Empty;
    }

    public WorkProject(int customerId, string name, string status, DateTime? dueDate)
    {
        CustomerId = customerId;
        Name = name;
        Status = status;
        DueDate = dueDate;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; private set; }

    public int CustomerId { get; private set; }

    public Customer? Customer { get; private set; }

    public string Name { get; private set; }

    public string Status { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public List<WorkTask> Tasks { get; private set; } = [];

    public List<ProjectDocument> Documents { get; private set; } = [];

    public void ChangeStatus(string status)
    {
        Status = status;
    }
}
