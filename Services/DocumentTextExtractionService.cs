using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using DeskFlowAI.Models;
using UglyToad.PdfPig;

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
                $"'{extension}' icin metin cikarma henuz eklenmedi. Bu adimda .txt, .docx, .pdf ve .xlsx dosyalari okunuyor.");
        }

        try
        {
            string text = ExtractTextByExtension(document.FilePath, extension);

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
            || string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractTextByExtension(string filePath, string extension)
    {
        if (string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractDocxText(filePath);
        }

        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractPdfText(filePath);
        }

        if (string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractXlsxText(filePath);
        }

        return File.ReadAllText(filePath);
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

    private static string ExtractPdfText(string filePath)
    {
        using PdfDocument document = PdfDocument.Open(filePath);
        IEnumerable<string> pageTexts = document
            .GetPages()
            .Select(page => page.Text)
            .Where(pageText => !string.IsNullOrWhiteSpace(pageText));

        return string.Join(Environment.NewLine, pageTexts);
    }

    private static string ExtractXlsxText(string filePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(filePath);
        List<string> sharedStrings = ReadXlsxSharedStrings(archive);
        IEnumerable<ZipArchiveEntry> sheetEntries = archive.Entries
            .Where(entry => entry.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase)
                || entry.FullName.StartsWith(@"xl\worksheets\", StringComparison.OrdinalIgnoreCase))
            .Where(entry => entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.FullName);

        List<string> rows = [];

        foreach (ZipArchiveEntry sheetEntry in sheetEntries)
        {
            rows.AddRange(ReadXlsxSheetRows(sheetEntry, sharedStrings));
        }

        return rows.Count == 0
            ? "Excel dosyasi okunabildi ancak metin iceren hucre bulunamadi."
            : string.Join(Environment.NewLine, rows);
    }

    private static List<string> ReadXlsxSharedStrings(ZipArchive archive)
    {
        ZipArchiveEntry? sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml")
            ?? archive.GetEntry(@"xl\sharedStrings.xml");

        if (sharedStringsEntry is null)
        {
            return [];
        }

        using Stream stream = sharedStringsEntry.Open();
        XDocument sharedStringsXml = XDocument.Load(stream);
        XNamespace spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return sharedStringsXml
            .Descendants(spreadsheetNamespace + "si")
            .Select(sharedString => string.Concat(
                sharedString
                    .Descendants(spreadsheetNamespace + "t")
                    .Select(textNode => textNode.Value)))
            .ToList();
    }

    private static IEnumerable<string> ReadXlsxSheetRows(ZipArchiveEntry sheetEntry, IReadOnlyList<string> sharedStrings)
    {
        using Stream stream = sheetEntry.Open();
        XDocument sheetXml = XDocument.Load(stream);
        XNamespace spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return sheetXml
            .Descendants(spreadsheetNamespace + "row")
            .Select(row => string.Join(" | ",
                row
                    .Elements(spreadsheetNamespace + "c")
                    .Select(cell => ReadXlsxCellValue(cell, sharedStrings))
                    .Where(value => !string.IsNullOrWhiteSpace(value))))
            .Where(rowText => !string.IsNullOrWhiteSpace(rowText))
            .ToList();
    }

    private static string ReadXlsxCellValue(XElement cell, IReadOnlyList<string> sharedStrings)
    {
        XNamespace spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        string cellType = cell.Attribute("t")?.Value ?? string.Empty;
        string rawValue = cell.Element(spreadsheetNamespace + "v")?.Value ?? string.Empty;

        if (cellType == "inlineStr")
        {
            return string.Concat(
                cell
                    .Descendants(spreadsheetNamespace + "t")
                    .Select(textNode => textNode.Value));
        }

        if (cellType == "s"
            && int.TryParse(rawValue, out int sharedStringIndex)
            && sharedStringIndex >= 0
            && sharedStringIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedStringIndex];
        }

        return rawValue;
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
