using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DocumentTextExtractionResult
{
    public DocumentTextExtractionResult(string status, string preview)
    {
        Status = status;
        Preview = preview;
    }

    public string Status { get; }

    public string Preview { get; }

    public bool IsSuccess => Status == DocumentTextExtractionStatusNames.Extracted;
}
