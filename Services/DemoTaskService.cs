using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoTaskService
{
    private readonly DeskFlowDbContext _dbContext = new();

    private static IQueryable<WorkTask> IncludeTaskDetails(IQueryable<WorkTask> query)
    {
        return query
            .Include(task => task.AssignedEmployee)
            .Include(task => task.Project)
            .ThenInclude(project => project!.Customer);
    }

    private static IOrderedQueryable<WorkTask> OrderTasks(IQueryable<WorkTask> query)
    {
        return query
            .OrderBy(task => task.DueDate == null)
            .ThenBy(task => task.DueDate)
            .ThenByDescending(task => task.Priority == TaskPriorityNames.Critical)
            .ThenByDescending(task => task.Priority == TaskPriorityNames.High)
            .ThenBy(task => task.Title);
    }

    public List<WorkTask> GetTasksForProject(int projectId)
    {
        IQueryable<WorkTask> query = _dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId);

        return OrderTasks(IncludeTaskDetails(query)).ToList();
    }

    public List<WorkTask> GetTasksForEmployee(int employeeId)
    {
        IQueryable<WorkTask> query = _dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.AssignedEmployeeId == employeeId);

        return OrderTasks(IncludeTaskDetails(query)).ToList();
    }

    public WorkTask CreateTask(int projectId, string title, string status, string priority, DateTime? dueDate, int? assignedEmployeeId)
    {
        WorkTask task = new(projectId, title, status, priority, dueDate, assignedEmployeeId);

        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        return task;
    }

    public WorkTask UpdateTaskWorkflow(WorkTask existingTask, string status, string priority, DateTime? dueDate, int? assignedEmployeeId)
    {
        WorkTask task = _dbContext.Tasks.Single(task => task.Id == existingTask.Id);
        task.ChangeWorkflow(status, priority, dueDate, assignedEmployeeId);
        _dbContext.SaveChanges();

        return task;
    }
}
