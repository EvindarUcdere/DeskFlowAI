using DeskFlowAI.Models;
using Microsoft.Extensions.Configuration;

namespace DeskFlowAI.Services;

public sealed class DocumentAIAnalysisService
{
    private readonly IDocumentAIAnalysisProvider _provider;

    public DocumentAIAnalysisService()
    {
        string providerName = LoadProviderName();
        _provider = CreateProvider(providerName);
    }

    public DocumentAIAnalysisResult Analyze(ProjectDocument document) => _provider.Analyze(document);

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
