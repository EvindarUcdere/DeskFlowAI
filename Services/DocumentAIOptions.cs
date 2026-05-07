namespace DeskFlowAI.Services;

public sealed class DocumentAIOptions
{
    public string Provider { get; init; } = DocumentAIProviderNames.RuleBased;

    public string OpenAIModel { get; init; } = string.Empty;

    public string OpenAIApiKeyEnvironmentVariable { get; init; } = "OPENAI_API_KEY";
}
