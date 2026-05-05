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

    public ProjectDocument AnalyzeDocument(ProjectDocument existingDocument)
    {
        ProjectDocument document = _dbContext.ProjectDocuments
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Single(document => document.Id == existingDocument.Id);

        string analysisStatus = document.Status == DocumentStatusNames.NeedsUpdate
            ? AIAnalysisStatusNames.NeedsReview
            : AIAnalysisStatusNames.Analyzed;

        string projectName = document.Project?.Name ?? "Unknown project";
        string customerName = document.Project?.Customer?.CompanyName ?? "Unknown customer";
        string summary = $"{document.FileName} belgesi {customerName} / {projectName} projesi icin kayitli. Mevcut belge status'u '{document.Status}'. Not: {BuildSafeNote(document.Notes)}";
        string riskNotes = BuildRiskNotes(document);

        document.UpdateAIAnalysis(analysisStatus, summary, riskNotes, DateTime.Now);
        _dbContext.SaveChanges();

        return document;
    }

    private static string BuildSafeNote(string notes)
    {
        return string.IsNullOrWhiteSpace(notes)
            ? "Belge notu girilmemis."
            : notes;
    }

    private static string BuildRiskNotes(ProjectDocument document)
    {
        if (document.Status == DocumentStatusNames.NeedsUpdate)
        {
            return "Belge guncelleme istiyor. Project tesliminden once sorumlu kisinin belgeyi revize etmesi onerilir.";
        }

        if (document.Status == DocumentStatusNames.InReview)
        {
            return "Belge inceleme asamasinda. Onay sureci gecikirse project teslim planini etkileyebilir.";
        }

        if (document.Status == DocumentStatusNames.Approved)
        {
            return "Belge onaylanmis. Kritik risk gorunmuyor; arsiv ve teslim kaydi kontrol edilmeli.";
        }

        return "Belge yuklenmis ancak henuz review sureci tamamlanmamis. Ilgili manager tarafindan kontrol edilmeli.";
    }
}
