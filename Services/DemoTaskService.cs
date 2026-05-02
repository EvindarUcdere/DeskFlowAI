using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoTaskService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<WorkTask> GetTasksForProject(int projectId)
    {
        return _dbContext.Tasks
            .AsNoTracking()
            .Include(task => task.Project)
            .ThenInclude(project => project!.Customer)
            .Where(task => task.ProjectId == projectId)
            .OrderBy(task => task.DueDate == null)
            .ThenBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority == TaskPriorityNames.Critical)
            .ThenByDescending(task => task.Priority == TaskPriorityNames.High)
            .ThenBy(task => task.Title)
            .ToList();
    }

    public WorkTask CreateTask(int projectId, string title, string status, string priority, DateTime? dueDate)
    {
        WorkTask task = new(projectId, title, status, priority, dueDate);

        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        return task;
    }

    public WorkTask UpdateTaskWorkflow(WorkTask existingTask, string status, string priority, DateTime? dueDate)
    {
        WorkTask task = _dbContext.Tasks.Single(task => task.Id == existingTask.Id);
        task.ChangeWorkflow(status, priority, dueDate);
        _dbContext.SaveChanges();

        return task;
    }
}
