using DeskFlowAI.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeskFlowAI.Services;

public sealed class OpenAIDocumentAIAnalysisProvider : IDocumentAIAnalysisProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
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

        try
        {
            return AnalyzeWithOpenAI(document, apiKey);
        }
        catch (Exception exception) when (exception is HttpRequestException
            or TaskCanceledException
            or JsonException
            or InvalidOperationException)
        {
            return BuildFallbackResult(
                document,
                $"OpenAI provider failed and rule-based fallback was used: {exception.Message}");
        }
    }

    private DocumentAIAnalysisResult AnalyzeWithOpenAI(ProjectDocument document, string apiKey)
    {
        using HttpClient httpClient = new()
        {
            BaseAddress = new Uri(BuildBaseUrl(_options.OpenAIBaseUrl)),
            Timeout = TimeSpan.FromSeconds(_options.OpenAITimeoutSeconds)
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        object request = BuildRequest(document);
        using HttpResponseMessage response = httpClient.PostAsJsonAsync("responses", request, JsonOptions)
            .GetAwaiter()
            .GetResult();
        string responseBody = response.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(BuildOpenAIErrorMessage(response, responseBody));
        }

        OpenAIAnalysisPayload payload = ParseAnalysisPayload(responseBody);

        return new DocumentAIAnalysisResult(
            NormalizeStatus(payload.Status),
            Truncate(payload.Summary, 1200),
            Truncate(payload.RiskNotes, 1200),
            DocumentAIProviderNames.OpenAI);
    }

    private object BuildRequest(ProjectDocument document)
    {
        return new
        {
            model = _options.OpenAIModel,
            instructions = "You are a careful document review assistant for an internal project operations tool. Return concise Turkish analysis. Do not invent facts. Flag uncertainty clearly.",
            input = BuildPrompt(document),
            max_output_tokens = _options.OpenAIMaxOutputTokens,
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "document_ai_analysis",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            status = new
                            {
                                type = "string",
                                @enum = new[] { AIAnalysisStatusNames.Analyzed, AIAnalysisStatusNames.NeedsReview, AIAnalysisStatusNames.Failed }
                            },
                            summary = new
                            {
                                type = "string",
                                description = "A concise Turkish summary for project managers."
                            },
                            risk_notes = new
                            {
                                type = "string",
                                description = "Concise Turkish risk notes and recommended follow-up actions."
                            }
                        },
                        required = new[] { "status", "summary", "risk_notes" }
                    }
                }
            }
        };
    }

    private static string BuildPrompt(ProjectDocument document)
    {
        string customerName = document.Project?.Customer?.CompanyName ?? "Unknown customer";
        string projectName = document.Project?.Name ?? "Unknown project";
        string extractedText = string.IsNullOrWhiteSpace(document.ExtractedTextPreview)
            ? "Extracted text is not available. Use metadata only and mention this limitation."
            : document.ExtractedTextPreview;

        return $"""
            Analyze this project document.

            Customer: {customerName}
            Project: {projectName}
            File name: {document.FileName}
            Document status: {document.Status}
            AI processing policy: {document.AIProcessingPolicy}
            Uploaded by: {document.UploadedByEmail}
            Notes: {BuildSafeText(document.Notes)}

            Extracted text preview:
            {extractedText}

            Decide status as:
            - Analyzed: no major follow-up risk is visible
            - Needs Review: approval, compliance, delay, missing information, blocked work, or update risk is visible
            - Failed: the content cannot be meaningfully analyzed
            """;
    }

    private static OpenAIAnalysisPayload ParseAnalysisPayload(string responseBody)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
        string outputText = ExtractOutputText(jsonDocument.RootElement);

        if (string.IsNullOrWhiteSpace(outputText))
        {
            throw new InvalidOperationException("OpenAI response did not include output text.");
        }

        OpenAIAnalysisPayload? payload = JsonSerializer.Deserialize<OpenAIAnalysisPayload>(outputText, JsonOptions);

        if (payload is null
            || string.IsNullOrWhiteSpace(payload.Status)
            || string.IsNullOrWhiteSpace(payload.Summary)
            || string.IsNullOrWhiteSpace(payload.RiskNotes))
        {
            throw new InvalidOperationException("OpenAI response JSON was incomplete.");
        }

        return payload;
    }

    private static string ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out JsonElement outputText)
            && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out JsonElement output)
            || output.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        List<string> textParts = [];

        foreach (JsonElement outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out JsonElement content)
                || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (JsonElement contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out JsonElement text)
                    && text.ValueKind == JsonValueKind.String)
                {
                    textParts.Add(text.GetString() ?? string.Empty);
                }
            }
        }

        return string.Join(Environment.NewLine, textParts);
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim() switch
        {
            AIAnalysisStatusNames.Analyzed => AIAnalysisStatusNames.Analyzed,
            AIAnalysisStatusNames.NeedsReview => AIAnalysisStatusNames.NeedsReview,
            AIAnalysisStatusNames.Failed => AIAnalysisStatusNames.Failed,
            _ => AIAnalysisStatusNames.NeedsReview
        };
    }

    private static string BuildBaseUrl(string baseUrl)
    {
        string value = string.IsNullOrWhiteSpace(baseUrl)
            ? "https://api.openai.com/v1"
            : baseUrl.Trim();

        return value.EndsWith("/", StringComparison.Ordinal)
            ? value
            : $"{value}/";
    }

    private static string BuildOpenAIErrorMessage(HttpResponseMessage response, string responseBody)
    {
        string body = Truncate(responseBody, 300);
        return $"OpenAI API returned {(int)response.StatusCode} {response.ReasonPhrase}. {body}";
    }

    private static string BuildSafeText(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? "No notes were provided."
            : text;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();

        return trimmed.Length <= maxLength
            ? trimmed
            : $"{trimmed[..(maxLength - 3)]}...";
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

    private sealed class OpenAIAnalysisPayload
    {
        public string Status { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;

        [JsonPropertyName("risk_notes")]
        public string RiskNotes { get; init; } = string.Empty;
    }
}
