using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoProjectService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<WorkProject> GetProjectsForCustomer(int customerId)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.CustomerId == customerId)
            .OrderBy(project => project.DueDate == null)
            .ThenBy(project => project.DueDate)
            .ThenBy(project => project.Name)
            .ToList();
    }

    public List<WorkProject> GetAllProjects()
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Customer)
            .OrderBy(project => project.DueDate == null)
            .ThenBy(project => project.DueDate)
            .ThenBy(project => project.Name)
            .ToList();
    }

    public List<WorkProject> GetProjectsDueWithinDays(int days)
    {
        DateTime today = DateTime.Today;
        DateTime limit = today.AddDays(days);

        return _dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Customer)
            .Where(project => project.DueDate.HasValue
                && project.DueDate.Value.Date >= today
                && project.DueDate.Value.Date <= limit
                && project.Status != ProjectStatusNames.Completed)
            .OrderBy(project => project.DueDate)
            .ToList();
    }

    public WorkProject CreateProject(int customerId, string name, string status, DateTime? dueDate)
    {
        WorkProject project = new(customerId, name, status, dueDate);

        _dbContext.Projects.Add(project);
        _dbContext.SaveChanges();

        return project;
    }

    public WorkProject UpdateProjectStatus(WorkProject existingProject, string status)
    {
        WorkProject project = _dbContext.Projects.Single(project => project.Id == existingProject.Id);
        project.ChangeStatus(status);
        _dbContext.SaveChanges();

        return project;
    }
}
