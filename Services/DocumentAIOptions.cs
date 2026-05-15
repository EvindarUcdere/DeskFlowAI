namespace DeskFlowAI.Services;

public sealed class DocumentAIOptions
{
    public string Provider { get; init; } = DocumentAIProviderNames.RuleBased;

    public string OpenAIModel { get; init; } = string.Empty;

    public string OpenAIApiKeyEnvironmentVariable { get; init; } = "OPENAI_API_KEY";

    public string OpenAIBaseUrl { get; init; } = "https://api.openai.com/v1";

    public int OpenAITimeoutSeconds { get; init; } = 30;

    public int OpenAIMaxOutputTokens { get; init; } = 700;
}
