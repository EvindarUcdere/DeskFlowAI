using DeskFlowAI.Models;
using Microsoft.Extensions.Configuration;

namespace DeskFlowAI.Services;

public sealed class DocumentAIAnalysisService
{
    private readonly IDocumentAIAnalysisProvider _configuredProvider;
    private readonly IDocumentAIAnalysisProvider _ruleBasedProvider = new RuleBasedDocumentAIAnalysisProvider();

    public DocumentAIAnalysisService()
    {
        string providerName = LoadProviderName();
        _configuredProvider = CreateProvider(providerName);
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

    private static string LoadProviderName()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        return configuration["DocumentAI:Provider"] ?? DocumentAIProviderNames.RuleBased;
    }

    private static IDocumentAIAnalysisProvider CreateProvider(string providerName)
    {
        if (string.Equals(providerName, DocumentAIProviderNames.RuleBased, StringComparison.OrdinalIgnoreCase))
        {
            return new RuleBasedDocumentAIAnalysisProvider();
        }

        return new RuleBasedDocumentAIAnalysisProvider();
    }
}
