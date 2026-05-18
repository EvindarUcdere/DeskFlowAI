namespace DeskFlowAI.Services;

public sealed class DocumentAIAnalysisResult
{
    public DocumentAIAnalysisResult(
        string status,
        string summary,
        string riskNotes,
        string providerName,
        bool usedFallback = false,
        string riskLevel = "",
        string recommendations = "",
        double? confidenceScore = null,
        string detectedIssues = "",
        int riskScore = 0,
        string complianceStatus = "",
        string policyViolations = "")
    {
        Status = status;
        Summary = summary;
        RiskNotes = riskNotes;
        ProviderName = providerName;
        UsedFallback = usedFallback;
        RiskLevel = riskLevel;
        Recommendations = recommendations;
        ConfidenceScore = confidenceScore;
        DetectedIssues = detectedIssues;
        RiskScore = riskScore;
        ComplianceStatus = complianceStatus;
        PolicyViolations = policyViolations;
    }

    public string Status { get; }

    public string Summary { get; }

    public string RiskNotes { get; }

    public string ProviderName { get; }

    public bool UsedFallback { get; }

    public string RiskLevel { get; }

    public string Recommendations { get; }

    public double? ConfidenceScore { get; }

    public string DetectedIssues { get; }

    public int RiskScore { get; }

    public string ComplianceStatus { get; }

    public string PolicyViolations { get; }
}
