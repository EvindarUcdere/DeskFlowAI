using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class DemoProjectDocumentService
{
    private readonly DeskFlowDbContext _dbContext = new();

    public List<ProjectDocument> GetDocumentsForProject(int projectId)
    {
        return _dbContext.ProjectDocuments
            .AsNoTracking()
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Where(document => document.ProjectId == projectId)
            .OrderByDescending(document => document.UploadedAt)
            .ThenBy(document => document.FileName)
            .ToList();
    }

    public List<ProjectDocument> GetDocumentsForEmployeeProjects(int employeeId)
    {
        return _dbContext.ProjectDocuments
            .AsNoTracking()
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Where(document => document.Project!.Tasks.Any(task => task.AssignedEmployeeId == employeeId))
            .OrderByDescending(document => document.UploadedAt)
            .ThenBy(document => document.FileName)
            .ToList();
    }

    public ProjectDocument CreateDocument(
        int projectId,
        string fileName,
        string filePath,
        string status,
        string uploadedByEmail,
        string notes)
    {
        ProjectDocument document = new(
            projectId,
            fileName,
            filePath,
            status,
            uploadedByEmail,
            notes);

        _dbContext.ProjectDocuments.Add(document);
        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument UpdateDocumentStatus(ProjectDocument existingDocument, string status, string notes)
    {
        ProjectDocument document = _dbContext.ProjectDocuments.Single(document => document.Id == existingDocument.Id);
        document.UpdateStatus(status, notes);
        _dbContext.SaveChanges();

        return document;
    }
}
