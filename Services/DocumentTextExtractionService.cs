using System.IO;
using DeskFlowAI.Models;

namespace DeskFlowAI.Services;

public sealed class DocumentTextExtractionService
{
    public DocumentTextExtractionResult Extract(ProjectDocument document)
    {
        string extension = Path.GetExtension(document.FilePath);

        if (document.FileCheckStatus != DocumentFileCheckStatusNames.Ready)
        {
            return new DocumentTextExtractionResult(
                DocumentTextExtractionStatusNames.FileNotReady,
                "Metin cikarmadan once dosya kontrolu Ready olmalidir.");
        }

        if (!string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return new DocumentTextExtractionResult(
                DocumentTextExtractionStatusNames.UnsupportedFile,
                $"'{extension}' icin metin cikarma henuz eklenmedi. Bu adimda sadece .txt dosyalari okunuyor.");
        }

        try
        {
            string text = File.ReadAllText(document.FilePath);

            return new DocumentTextExtractionResult(
                DocumentTextExtractionStatusNames.Extracted,
                BuildTextPreview(text));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new DocumentTextExtractionResult(
                DocumentTextExtractionStatusNames.ExtractError,
                $"Dosyadan metin okunamadi: {exception.Message}");
        }
    }

    private static string BuildTextPreview(string text)
    {
        string normalizedText = string.Join(
            " ",
            text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return "Dosya okunabildi ancak metin icerigi bos gorunuyor.";
        }

        return normalizedText.Length <= 1500
            ? normalizedText
            : $"{normalizedText[..1500]}...";
    }
}
