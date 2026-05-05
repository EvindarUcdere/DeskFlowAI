namespace DeskFlowAI.Models;

public sealed class ProjectDocument
{
    private ProjectDocument()
    {
        FileName = string.Empty;
        FilePath = string.Empty;
        Status = string.Empty;
        UploadedByEmail = string.Empty;
        Notes = string.Empty;
        AIAnalysisStatus = string.Empty;
        AISummary = string.Empty;
        AIRiskNotes = string.Empty;
        FileCheckStatus = string.Empty;
        FileCheckMessage = string.Empty;
        TextExtractionStatus = string.Empty;
        ExtractedTextPreview = string.Empty;
    }

    public ProjectDocument(
        int projectId,
        string fileName,
        string filePath,
        string status,
        string uploadedByEmail,
        string notes)
    {
        ProjectId = projectId;
        FileName = fileName;
        FilePath = filePath;
        Status = status;
        UploadedByEmail = uploadedByEmail;
        Notes = notes;
        UploadedAt = DateTime.Now;
        AIAnalysisStatus = AIAnalysisStatusNames.NotAnalyzed;
        AISummary = string.Empty;
        AIRiskNotes = string.Empty;
        FileCheckStatus = DocumentFileCheckStatusNames.NotChecked;
        FileCheckMessage = string.Empty;
        TextExtractionStatus = DocumentTextExtractionStatusNames.NotExtracted;
        ExtractedTextPreview = string.Empty;
    }

    public int Id { get; private set; }

    public int ProjectId { get; private set; }

    public WorkProject? Project { get; private set; }

    public string FileName { get; private set; }

    public string FilePath { get; private set; }

    public string Status { get; private set; }

    public string UploadedByEmail { get; private set; }

    public DateTime UploadedAt { get; private set; }

    public string Notes { get; private set; }

    public string AIAnalysisStatus { get; private set; }

    public string AISummary { get; private set; }

    public string AIRiskNotes { get; private set; }

    public DateTime? AnalyzedAt { get; private set; }

    public string FileCheckStatus { get; private set; }

    public string FileCheckMessage { get; private set; }

    public DateTime? FileCheckedAt { get; private set; }

    public string TextExtractionStatus { get; private set; }

    public string ExtractedTextPreview { get; private set; }

    public DateTime? TextExtractedAt { get; private set; }

    public void UpdateStatus(string status, string notes)
    {
        Status = status;
        Notes = notes;
    }

    public void UpdateAIAnalysis(string analysisStatus, string summary, string riskNotes, DateTime? analyzedAt)
    {
        AIAnalysisStatus = analysisStatus;
        AISummary = summary;
        AIRiskNotes = riskNotes;
        AnalyzedAt = analyzedAt;
    }

    public void UpdateFileCheck(string fileCheckStatus, string fileCheckMessage, DateTime? fileCheckedAt)
    {
        FileCheckStatus = fileCheckStatus;
        FileCheckMessage = fileCheckMessage;
        FileCheckedAt = fileCheckedAt;
    }

    public void UpdateTextExtraction(string textExtractionStatus, string extractedTextPreview, DateTime? textExtractedAt)
    {
        TextExtractionStatus = textExtractionStatus;
        ExtractedTextPreview = extractedTextPreview;
        TextExtractedAt = textExtractedAt;
    }
}
