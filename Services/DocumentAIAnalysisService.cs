using DeskFlowAI.Models;
using Microsoft.Extensions.Configuration;

namespace DeskFlowAI.Services;

public sealed class DocumentAIAnalysisService
{
    private readonly IDocumentAIAnalysisProvider _configuredProvider;
    private readonly IDocumentAIAnalysisProvider _ruleBasedProvider = new RuleBasedDocumentAIAnalysisProvider();

    public DocumentAIAnalysisService()
    {
        DocumentAIOptions options = LoadOptions();
        _configuredProvider = CreateProvider(options, _ruleBasedProvider);
    }

    public DocumentAIAnalysisResult Analyze(ProjectDocument document)
    {
        if (document.AIProcessingPolicy is DocumentAIProcessingPolicyNames.InternalOnly
            or DocumentAIProcessingPolicyNames.NeedsApproval)
        {
            return _ruleBasedProvider.Analyze(document);
        }

        return _configuredProvider.Analyze(document);
    }

    private static DocumentAIOptions LoadOptions()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        IConfigurationSection section = configuration.GetSection("DocumentAI");

        return new DocumentAIOptions
        {
            Provider = section["Provider"] ?? DocumentAIProviderNames.RuleBased,
            OpenAIModel = section["OpenAIModel"] ?? string.Empty,
            OpenAIApiKeyEnvironmentVariable = section["OpenAIApiKeyEnvironmentVariable"] ?? "OPENAI_API_KEY",
            OpenAIBaseUrl = section["OpenAIBaseUrl"] ?? "https://api.openai.com/v1",
            OpenAITimeoutSeconds = ReadPositiveInt(section, "OpenAITimeoutSeconds", 30),
            OpenAIMaxOutputTokens = ReadPositiveInt(section, "OpenAIMaxOutputTokens", 700)
        };
    }

    private static int ReadPositiveInt(IConfigurationSection section, string key, int fallback)
    {
        return int.TryParse(section[key], out int value) && value > 0
            ? value
            : fallback;
    }

    private static IDocumentAIAnalysisProvider CreateProvider(DocumentAIOptions options, IDocumentAIAnalysisProvider fallbackProvider)
    {
        if (string.Equals(options.Provider, DocumentAIProviderNames.OpenAI, StringComparison.OrdinalIgnoreCase))
        {
            return new OpenAIDocumentAIAnalysisProvider(options, fallbackProvider);
        }

        return fallbackProvider;
    }
}
