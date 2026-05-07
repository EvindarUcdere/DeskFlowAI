using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class OpenAIDocumentAIAnalysisProvider : IDocumentAIAnalysisProvider
{
    private readonly DocumentAIOptions _options;
    private readonly IDocumentAIAnalysisProvider _fallbackProvider;

    public OpenAIDocumentAIAnalysisProvider(DocumentAIOptions options, IDocumentAIAnalysisProvider fallbackProvider)
    {
        _options = options;
        _fallbackProvider = fallbackProvider;
    }

    public DocumentAIAnalysisResult Analyze(ProjectDocument document)
    {
        string apiKey = Environment.GetEnvironmentVariable(_options.OpenAIApiKeyEnvironmentVariable) ?? string.Empty;

        if (document.AIProcessingPolicy != DocumentAIProcessingPolicyNames.ExternalAIAllowed)
        {
            return BuildFallbackResult(
                document,
                $"OpenAI provider skipped because document policy is '{document.AIProcessingPolicy}'.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildFallbackResult(
                document,
                $"OpenAI provider skipped because environment variable '{_options.OpenAIApiKeyEnvironmentVariable}' is not set.");
        }

        if (string.IsNullOrWhiteSpace(_options.OpenAIModel))
        {
            return BuildFallbackResult(
                document,
                "OpenAI provider skipped because DocumentAI:OpenAIModel is not configured.");
        }

        return BuildFallbackResult(
            document,
            "OpenAI provider is configured, but the real API call is not enabled yet.");
    }

    private DocumentAIAnalysisResult BuildFallbackResult(ProjectDocument document, string reason)
    {
        DocumentAIAnalysisResult fallbackResult = _fallbackProvider.Analyze(document);

        return new DocumentAIAnalysisResult(
            fallbackResult.Status,
            $"{fallbackResult.Summary} OpenAI fallback: {reason}",
            fallbackResult.RiskNotes,
            $"{DocumentAIProviderNames.OpenAI} -> {fallbackResult.ProviderName}");
    }
}
