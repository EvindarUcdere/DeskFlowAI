using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class MockDocumentAIAnalysisProvider : IDocumentAIAnalysisProvider
{
    private static readonly string[] HighRiskKeywords =
    [
        "delay",
        "penalty",
        "breach",
        "termination",
        "overdue",
        "missing",
        "failed"
    ];

    private static readonly string[] MediumRiskKeywords =
    [
        "refund",
        "revision",
        "pending",
        "approval",
        "dependency"
    ];

    public DocumentAIAnalysisResult Analyze(ProjectDocument document)
    {
        string analysisText = BuildAnalysisText(document);
        List<string> highRiskIssues = DetectIssues(analysisText, HighRiskKeywords);
        List<string> mediumRiskIssues = DetectIssues(analysisText, MediumRiskKeywords);
        string riskLevel = DetermineRiskLevel(highRiskIssues, mediumRiskIssues);
        string status = riskLevel == "Low"
            ? AIAnalysisStatusNames.Analyzed
            : AIAnalysisStatusNames.NeedsReview;
        string detectedIssues = BuildDetectedIssues(highRiskIssues, mediumRiskIssues);
        double confidenceScore = DetermineConfidenceScore(riskLevel, detectedIssues);
        string projectName = document.Project?.Name ?? "Unknown project";
        string customerName = document.Project?.Customer?.CompanyName ?? "Unknown customer";

        string summary = $"Mock AI analizi: {document.FileName} belgesi {customerName} / {projectName} projesi icin incelendi. Risk seviyesi: {riskLevel}. Belge status'u '{document.Status}', AI policy '{document.AIProcessingPolicy}'.";
        string riskNotes = riskLevel switch
        {
            "High" => $"High risk: {detectedIssues}. Teslim plani, sozlesme sorumluluklari ve musteri onayi hemen kontrol edilmeli.",
            "Medium" => $"Medium risk: {detectedIssues}. Sorumlu kisi, bagimliliklar ve onay akisi takip edilmeli.",
            _ => "Low risk: Belge metninde belirgin operasyonel risk sinyali bulunmadi; rutin review sureci yeterli gorunuyor."
        };
        string recommendations = riskLevel switch
        {
            "High" => "Manager escalation ac, musteri onaylarini dogrula, teslim tarihini yeniden degerlendir ve risk kaydi olustur.",
            "Medium" => "Bekleyen onaylari ve dependency sahiplerini netlestir, revizyon ihtiyacini proje planina ekle.",
            _ => "Belgeyi normal review akisiyle ilerlet, kapanis veya arsiv kontrolunu tamamla."
        };

        return new DocumentAIAnalysisResult(
            status,
            summary,
            riskNotes,
            DocumentAIProviderNames.MockAI,
            usedFallback: false,
            riskLevel: riskLevel,
            recommendations: recommendations,
            confidenceScore: confidenceScore,
            detectedIssues: detectedIssues);
    }

    private static string BuildAnalysisText(ProjectDocument document)
    {
        return string.Join(
            " ",
            document.FileName,
            document.Status,
            document.Notes,
            document.ExtractedTextPreview).ToLowerInvariant();
    }

    private static List<string> DetectIssues(string text, IEnumerable<string> keywords)
    {
        return keywords
            .Where(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(keyword => keyword)
            .ToList();
    }

    private static string DetermineRiskLevel(IReadOnlyCollection<string> highRiskIssues, IReadOnlyCollection<string> mediumRiskIssues)
    {
        if (highRiskIssues.Count > 0)
        {
            return "High";
        }

        return mediumRiskIssues.Count > 0 ? "Medium" : "Low";
    }

    private static string BuildDetectedIssues(IReadOnlyCollection<string> highRiskIssues, IReadOnlyCollection<string> mediumRiskIssues)
    {
        List<string> issues = [];

        if (highRiskIssues.Count > 0)
        {
            issues.Add($"High risk keywords: {string.Join(", ", highRiskIssues)}");
        }

        if (mediumRiskIssues.Count > 0)
        {
            issues.Add($"Medium risk keywords: {string.Join(", ", mediumRiskIssues)}");
        }

        return issues.Count == 0
            ? "No configured mock risk keywords detected"
            : string.Join("; ", issues);
    }

    private static double DetermineConfidenceScore(string riskLevel, string detectedIssues)
    {
        if (riskLevel == "High")
        {
            return 0.92;
        }

        if (riskLevel == "Medium")
        {
            return 0.84;
        }

        return detectedIssues.Contains("No configured", StringComparison.OrdinalIgnoreCase) ? 0.74 : 0.78;
    }
}
