using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DeskFlowAI.Services;

public sealed class DemoProjectDocumentService
{
    private readonly DeskFlowDbContext _dbContext = new();
    private readonly DocumentTextExtractionService _textExtractionService = new();
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
        bool hasExtractedText = HasExtractedText(document);
        string summary = hasExtractedText
            ? BuildContentBasedSummary(document, customerName, projectName)
            : BuildMetadataBasedSummary(document, customerName, projectName);
        string riskNotes = hasExtractedText
            ? BuildContentBasedRiskNotes(document)
            : BuildMetadataBasedRiskNotes(document);

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

    private static bool HasExtractedText(ProjectDocument document)
    {
        return document.TextExtractionStatus == DocumentTextExtractionStatusNames.Extracted
            && !string.IsNullOrWhiteSpace(document.ExtractedTextPreview);
    }

    private static string BuildMetadataBasedSummary(ProjectDocument document, string customerName, string projectName)
    {
        return $"{document.FileName} belgesi {customerName} / {projectName} projesi icin kayitli. Analiz kaynagi: belge kaydi. Mevcut belge status'u '{document.Status}'. Not: {BuildSafeNote(document.Notes)}";
    }

    private static string BuildContentBasedSummary(ProjectDocument document, string customerName, string projectName)
    {
        return $"{document.FileName} belgesi {customerName} / {projectName} projesi icin cikartilmis belge metnine gore analiz edildi. Mevcut belge status'u '{document.Status}'. Metin onizleme: {BuildSummarySnippet(document.ExtractedTextPreview)}";
    }

    private static string BuildSummarySnippet(string text)
    {
        return text.Length <= 520
            ? text
            : $"{text[..520]}...";
    }

    private static string BuildMetadataBasedRiskNotes(ProjectDocument document)
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

    private static string BuildContentBasedRiskNotes(ProjectDocument document)
    {
        List<string> risks = [];
        string text = document.ExtractedTextPreview.ToLowerInvariant();

        if (document.Status == DocumentStatusNames.NeedsUpdate)
        {
            risks.Add("Belge status'u Needs Update oldugu icin revizyon aksiyonu gerekiyor.");
        }

        if (ContainsAny(text, "risk", "delay", "gecik", "blocked", "blok", "overdue"))
        {
            risks.Add("Metinde risk, gecikme veya blokaj sinyali var; project planina etkisi kontrol edilmeli.");
        }

        if (ContainsAny(text, "approve", "approval", "onay", "signoff", "imza"))
        {
            risks.Add("Metin onay veya imza surecine isaret ediyor; sorumlu kisi ve son tarih netlestirilmeli.");
        }

        if (ContainsAny(text, "kvkk", "gdpr", "personal data", "kisisel veri", "compliance", "uyum"))
        {
            risks.Add("Metinde uyumluluk veya kisisel veri konusu geciyor; hukuki/operasyonel kontrol gerekebilir.");
        }

        if (ContainsAny(text, "stock", "stok", "return", "iade", "alarm", "incident", "olay"))
        {
            risks.Add("Metin operasyonel takip gerektiren stok, iade, alarm veya olay bilgileri iceriyor.");
        }

        if (risks.Count == 0)
        {
            risks.Add("Cikartilan metinde belirgin kritik risk sinyali bulunmadi; yine de manager review sureci tamamlanmali.");
        }

        return string.Join(" ", risks);
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(text.Contains);
    }
}
