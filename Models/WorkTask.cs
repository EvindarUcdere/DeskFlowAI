namespace DeskFlowAI.Models;

public sealed class WorkTask
{
    private WorkTask()
    {
        Title = string.Empty;
        Status = string.Empty;
        Priority = string.Empty;
    }

    public WorkTask(int projectId, string title, string status, string priority, DateTime? dueDate)
    {
        ProjectId = projectId;
        Title = title;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; private set; }

    public int ProjectId { get; private set; }

    public WorkProject? Project { get; private set; }

    public string Title { get; private set; }

    public string Status { get; private set; }

    public string Priority { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void ChangeWorkflow(string status, string priority, DateTime? dueDate)
    {
        Status = status;
        Priority = priority;
        DueDate = dueDate;
    }
}
