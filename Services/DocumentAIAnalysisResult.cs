namespace DeskFlowAI.Services;

public sealed class DocumentAIAnalysisResult
{
    public DocumentAIAnalysisResult(string status, string summary, string riskNotes, string providerName)
    {
        Status = status;
        Summary = summary;
        RiskNotes = riskNotes;
        ProviderName = providerName;
    }

    public string Status { get; }

    public string Summary { get; }

    public string RiskNotes { get; }

    public string ProviderName { get; }
}
