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
            .OrderBy(project => project.Name)
            .ToList();
    }

    public WorkProject CreateProject(int customerId, string name, string status)
    {
        WorkProject project = new(customerId, name, status);

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
