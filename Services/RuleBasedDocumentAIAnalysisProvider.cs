using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class RuleBasedDocumentAIAnalysisProvider : IDocumentAIAnalysisProvider
{
    public DocumentAIAnalysisResult Analyze(ProjectDocument document)
    {
        string analysisStatus = document.Status == DocumentStatusNames.NeedsUpdate
            || document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval
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
        string riskLevel = DetermineRiskLevel(document, riskNotes, analysisStatus);
        int riskScore = DetermineRiskScore(riskLevel, riskNotes);
        string complianceStatus = DetermineComplianceStatus(document, riskNotes);
        string policyViolations = BuildPolicyViolations(document, riskNotes);

        return new DocumentAIAnalysisResult(
            analysisStatus,
            summary,
            riskNotes,
            DocumentAIProviderNames.RuleBased,
            usedFallback: false,
            riskLevel: riskLevel,
            recommendations: analysisStatus == AIAnalysisStatusNames.NeedsReview
                ? "Manager review surecini tamamla ve belge status'unu teslim planina gore guncelle."
                : "Rutin belge review surecini tamamla.",
            confidenceScore: hasExtractedText ? 0.68 : 0.55,
            detectedIssues: riskNotes,
            riskScore: riskScore,
            complianceStatus: complianceStatus,
            policyViolations: policyViolations);
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
        return $"{document.FileName} belgesi {customerName} / {projectName} projesi icin kayitli. Analiz kaynagi: belge kaydi. AI policy: {document.AIProcessingPolicy}. Mevcut belge status'u '{document.Status}'. Not: {BuildSafeNote(document.Notes)}";
    }

    private static string BuildContentBasedSummary(ProjectDocument document, string customerName, string projectName)
    {
        return $"{document.FileName} belgesi {customerName} / {projectName} projesi icin cikartilmis belge metnine gore analiz edildi. AI policy: {document.AIProcessingPolicy}. Mevcut belge status'u '{document.Status}'. Metin onizleme: {BuildSummarySnippet(document.ExtractedTextPreview)}";
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

        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval)
        {
            return "Belge dis AI kullanimi icin onay gerektiriyor. Bu asamada yalnizca internal rule-based analiz uygulanmalidir.";
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

        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval)
        {
            risks.Add("Belge dis AI kullanimi icin onay gerektiriyor; harici provider'a gonderilmemeli.");
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

    private static string DetermineRiskLevel(ProjectDocument document, string riskNotes, string analysisStatus)
    {
        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.Blocked
            || ContainsAny(riskNotes.ToLowerInvariant(), "kisisel veri", "uyumluluk", "hukuki", "gecikme", "blokaj"))
        {
            return "High";
        }

        return analysisStatus == AIAnalysisStatusNames.NeedsReview ? "Medium" : "Low";
    }

    private static int DetermineRiskScore(string riskLevel, string riskNotes)
    {
        int score = riskLevel switch
        {
            "High" => 78,
            "Medium" => 52,
            _ => 22
        };

        if (riskNotes.Contains("onay", StringComparison.OrdinalIgnoreCase)
            || riskNotes.Contains("approval", StringComparison.OrdinalIgnoreCase))
        {
            score += 8;
        }

        return Math.Min(score, 100);
    }

    private static string DetermineComplianceStatus(ProjectDocument document, string riskNotes)
    {
        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.Blocked
            || riskNotes.Contains("kisisel veri", StringComparison.OrdinalIgnoreCase)
            || riskNotes.Contains("uyumluluk", StringComparison.OrdinalIgnoreCase)
            || riskNotes.Contains("hukuki", StringComparison.OrdinalIgnoreCase))
        {
            return AIComplianceStatusNames.ViolationDetected;
        }

        return document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval
            ? AIComplianceStatusNames.ReviewRequired
            : AIComplianceStatusNames.Passed;
    }

    private static string BuildPolicyViolations(ProjectDocument document, string riskNotes)
    {
        List<string> violations = [];

        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.NeedsApproval)
        {
            violations.Add("External AI approval is required before external processing.");
        }

        if (document.AIProcessingPolicy == DocumentAIProcessingPolicyNames.Blocked)
        {
            violations.Add("AI processing is blocked by policy.");
        }

        if (riskNotes.Contains("kisisel veri", StringComparison.OrdinalIgnoreCase)
            || riskNotes.Contains("uyumluluk", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add("Compliance or personal data wording detected.");
        }

        return violations.Count == 0
            ? "No policy violations detected"
            : string.Join("; ", violations);
    }
}
