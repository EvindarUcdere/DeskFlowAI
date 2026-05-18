using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DeskFlowAI.Services;

public sealed class DemoProjectDocumentService
{
    private readonly DeskFlowDbContext _dbContext = new();
    private readonly DocumentTextExtractionService _textExtractionService = new();
    private readonly DocumentAIAnalysisService _aiAnalysisService = new();
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".txt"
    };

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
        string aiProcessingPolicy,
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
        document.UpdateAIProcessingPolicy(aiProcessingPolicy);

        _dbContext.ProjectDocuments.Add(document);
        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument CheckDocumentFile(ProjectDocument existingDocument)
    {
        ProjectDocument document = _dbContext.ProjectDocuments
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Single(document => document.Id == existingDocument.Id);

        string extension = Path.GetExtension(document.FilePath);

        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            document.UpdateFileCheck(
                DocumentFileCheckStatusNames.FileMissing,
                "Dosya yolu bos. Analiz icin once geceli bir file path girilmeli.",
                DateTime.Now);
        }
        else if (!File.Exists(document.FilePath))
        {
            document.UpdateFileCheck(
                DocumentFileCheckStatusNames.FileMissing,
                $"Dosya bulunamadi: {document.FilePath}",
                DateTime.Now);
        }
        else if (!SupportedExtensions.Contains(extension))
        {
            document.UpdateFileCheck(
                DocumentFileCheckStatusNames.UnsupportedFile,
                $"'{extension}' uzantisi henuz desteklenmiyor. Desteklenenler: {string.Join(", ", SupportedExtensions.OrderBy(value => value))}.",
                DateTime.Now);
        }
        else
        {
            try
            {
                using FileStream stream = File.Open(document.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                document.UpdateFileCheck(
                    DocumentFileCheckStatusNames.Ready,
                    $"Dosya bulundu ve okunabilir. Boyut: {stream.Length:N0} byte. Uzanti: {extension}.",
                    DateTime.Now);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                document.UpdateFileCheck(
                    DocumentFileCheckStatusNames.ReadError,
                    $"Dosya bulundu ama okunamadi: {exception.Message}",
                    DateTime.Now);
            }
        }

        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument ExtractDocumentText(ProjectDocument existingDocument)
    {
        ProjectDocument document = _dbContext.ProjectDocuments
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Single(document => document.Id == existingDocument.Id);

        DocumentTextExtractionResult result = _textExtractionService.Extract(document);
        document.UpdateTextExtraction(result.Status, result.Preview, DateTime.Now);

        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument UpdateDocumentStatus(ProjectDocument existingDocument, string status, string aiProcessingPolicy, string notes)
    {
        ProjectDocument document = _dbContext.ProjectDocuments.Single(document => document.Id == existingDocument.Id);
        document.UpdateStatus(status, notes);
        document.UpdateAIProcessingPolicy(aiProcessingPolicy);
        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument ApproveExternalAIProcessing(ProjectDocument existingDocument)
    {
        ProjectDocument document = _dbContext.ProjectDocuments.Single(document => document.Id == existingDocument.Id);
        document.ApproveExternalAIProcessing();
        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument AnalyzeDocument(ProjectDocument existingDocument)
    {
        ProjectDocument document = _dbContext.ProjectDocuments
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Single(document => document.Id == existingDocument.Id);

        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.Blocked)
        {
            document.UpdateAIAnalysis(
                AIAnalysisStatusNames.Blocked,
                "AI analizi bu belge icin policy tarafindan engellendi.",
                "Belge AI Processing Policy = Blocked durumunda. Analiz yapmak icin yetkili bir yonetici policy bilgisini guncellemelidir.",
                DateTime.Now,
                DocumentAIProviderNames.RuleBased,
                usedFallback: false,
                riskLevel: "Blocked",
                recommendations: "Yetkili bir yonetici belge AI policy bilgisini guncellemeden analiz calistirilamaz.",
                confidenceScore: 1,
                detectedIssues: "AI policy is Blocked");
            _dbContext.SaveChanges();

            return document;
        }

        DocumentAIAnalysisResult result = _aiAnalysisService.Analyze(document);
        document.UpdateAIAnalysis(
            result.Status,
            result.Summary,
            result.RiskNotes,
            DateTime.Now,
            result.ProviderName,
            result.UsedFallback,
            result.RiskLevel,
            result.Recommendations,
            result.ConfidenceScore,
            result.DetectedIssues);
        _dbContext.SaveChanges();

        return document;
    }

    public ProjectDocument MarkAIReviewAsReviewed(ProjectDocument existingDocument, string reviewedByEmail)
    {
        ProjectDocument document = _dbContext.ProjectDocuments
            .Include(document => document.Project)
            .ThenInclude(project => project!.Customer)
            .Single(document => document.Id == existingDocument.Id);

        document.MarkAIReviewAsReviewed(reviewedByEmail);
        _dbContext.SaveChanges();

        return document;
    }
}
