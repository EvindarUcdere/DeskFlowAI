using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
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

        if (!IsExtractionSupported(extension))
        {
            return new DocumentTextExtractionResult(
                DocumentTextExtractionStatusNames.UnsupportedFile,
                $"'{extension}' icin metin cikarma henuz eklenmedi. Bu adimda .txt ve .docx dosyalari okunuyor.");
        }

        try
        {
            string text = string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase)
                ? ExtractDocxText(document.FilePath)
                : File.ReadAllText(document.FilePath);

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

    private static bool IsExtractionSupported(string extension)
    {
        return string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractDocxText(string filePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(filePath);
        ZipArchiveEntry? documentEntry = archive.GetEntry("word/document.xml")
            ?? archive.GetEntry(@"word\document.xml");

        if (documentEntry is null)
        {
            return "Word belgesi okunabildi ancak word/document.xml bulunamadi.";
        }

        using Stream stream = documentEntry.Open();
        XDocument documentXml = XDocument.Load(stream);
        XNamespace wordNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        IEnumerable<string> paragraphs = documentXml
            .Descendants(wordNamespace + "p")
            .Select(paragraph => string.Concat(
                paragraph
                    .Descendants(wordNamespace + "t")
                    .Select(textNode => textNode.Value)))
            .Where(paragraphText => !string.IsNullOrWhiteSpace(paragraphText));

        return string.Join(Environment.NewLine, paragraphs);
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
