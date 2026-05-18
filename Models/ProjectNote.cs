namespace DeskFlowAI.Models;

public sealed class ProjectNote
{
    private ProjectNote()
    {
        Message = string.Empty;
        CreatedByEmail = string.Empty;
    }

    public ProjectNote(int projectId, string message, string createdByEmail)
    {
        ProjectId = projectId;
        Message = message;
        CreatedByEmail = createdByEmail;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; private set; }

    public int ProjectId { get; private set; }

    public WorkProject? Project { get; private set; }

    public string Message { get; private set; }

    public string CreatedByEmail { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
