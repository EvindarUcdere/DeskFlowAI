using System.Globalization;
using System.IO;
using System.Text;
using DeskFlowAI.Data;
using DeskFlowAI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlowAI.Services;

public sealed class ProjectReportExportService
{
    public void ExportProjectReport(int projectId, string outputPath)
    {
        using DeskFlowDbContext dbContext = new();
        WorkProject project = dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Customer)
            .Include(project => project.Tasks)
            .ThenInclude(task => task.AssignedEmployee)
            .Include(project => project.Documents)
            .Single(project => project.Id == projectId);

        List<string> lines = BuildReportLines(project);
        SimplePdfWriter.Write(outputPath, lines);
    }

    private static List<string> BuildReportLines(WorkProject project)
    {
        List<string> lines =
        [
            "DeskFlow AI Project Report",
            $"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}",
            "",
            "Project",
            $"Name: {project.Name}",
            $"Customer: {project.Customer?.CompanyName ?? "Unknown customer"}",
            $"Status: {project.Status}",
            $"Due date: {FormatDate(project.DueDate)}",
            "",
            "Task Summary",
            $"Total tasks: {project.Tasks.Count}",
            $"Open tasks: {project.Tasks.Count(task => task.Status != TaskStatusNames.Done)}",
            $"Done tasks: {project.Tasks.Count(task => task.Status == TaskStatusNames.Done)}",
            $"Overdue tasks: {project.Tasks.Count(task => task.IsOverdue)}",
            ""
        ];

        foreach (WorkTask task in project.Tasks.OrderBy(task => task.DueDate == null).ThenBy(task => task.DueDate).ThenBy(task => task.Title))
        {
            lines.Add($"- {task.Title}");
            lines.Add($"  Status: {task.Status} | Priority: {task.Priority} | Assignee: {task.AssignedEmployee?.FullName ?? "Unassigned"} | Due: {FormatDate(task.DueDate)}");

            if (!string.IsNullOrWhiteSpace(task.BlockedBy))
            {
                lines.Add($"  Dependency: {task.BlockedBy}");
            }
        }

        lines.Add("");
        lines.Add("Document And AI Summary");
        lines.Add($"Total documents: {project.Documents.Count}");
        lines.Add($"Analyzed documents: {project.Documents.Count(document => document.AIAnalysisStatus != AIAnalysisStatusNames.NotAnalyzed)}");
        lines.Add("");

        foreach (ProjectDocument document in project.Documents.OrderByDescending(document => document.UploadedAt).ThenBy(document => document.FileName))
        {
            lines.Add($"- {document.FileName}");
            lines.Add($"  Status: {document.Status} | AI Policy: {document.AIProcessingPolicy} | AI Status: {document.AIAnalysisStatus}");
            lines.Add($"  Provider: {EmptyAsNone(document.AIProviderName)} | Risk: {EmptyAsNone(document.AIRiskLevel)} | Score: {document.AIRiskScore} | Compliance: {EmptyAsNone(document.AIComplianceStatus)}");

            if (!string.IsNullOrWhiteSpace(document.AISummary))
            {
                lines.Add($"  Summary: {document.AISummary}");
            }

            if (!string.IsNullOrWhiteSpace(document.AIRecommendations))
            {
                lines.Add($"  Recommendations: {document.AIRecommendations}");
            }
        }

        return lines;
    }

    private static string FormatDate(DateTime? date)
    {
        return date.HasValue
            ? date.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
            : "No due date";
    }

    private static string EmptyAsNone(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "None" : value;
    }
}

internal static class SimplePdfWriter
{
    public static void Write(string outputPath, IReadOnlyList<string> lines)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

        List<string> pageContents = BuildPageContents(lines);
        List<string> objects = [];
        int pageCount = pageContents.Count;
        int pagesObjectNumber = 2 + pageCount;
        int fontObjectNumber = pagesObjectNumber + 1;

        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

        for (int index = 0; index < pageCount; index++)
        {
            int contentObjectNumber = 2 + pageCount + 2 + index;
            objects.Add($"<< /Type /Page /Parent {pagesObjectNumber} 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 {fontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
        }

        string kids = string.Join(" ", Enumerable.Range(2, pageCount).Select(objectNumber => $"{objectNumber} 0 R"));
        objects.Add($"<< /Type /Pages /Kids [{kids}] /Count {pageCount} >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        foreach (string pageContent in pageContents)
        {
            byte[] contentBytes = Encoding.ASCII.GetBytes(pageContent);
            objects.Add($"<< /Length {contentBytes.Length} >>\nstream\n{pageContent}\nendstream");
        }

        using FileStream stream = File.Create(outputPath);
        using StreamWriter writer = new(stream, Encoding.ASCII, leaveOpen: true);

        writer.WriteLine("%PDF-1.4");
        writer.Flush();

        List<long> offsets = [0];

        for (int index = 0; index < objects.Count; index++)
        {
            offsets.Add(stream.Position);
            writer.WriteLine($"{index + 1} 0 obj");
            writer.WriteLine(objects[index]);
            writer.WriteLine("endobj");
            writer.Flush();
        }

        long xrefPosition = stream.Position;
        writer.WriteLine("xref");
        writer.WriteLine($"0 {objects.Count + 1}");
        writer.WriteLine("0000000000 65535 f ");

        for (int index = 1; index < offsets.Count; index++)
        {
            writer.WriteLine($"{offsets[index]:0000000000} 00000 n ");
        }

        writer.WriteLine("trailer");
        writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine(xrefPosition);
        writer.WriteLine("%%EOF");
    }

    private static List<string> BuildPageContents(IReadOnlyList<string> lines)
    {
        List<string> pages = [];
        StringBuilder page = new();
        int lineOnPage = 0;

        foreach (string line in lines.SelectMany(WrapLine))
        {
            if (lineOnPage == 0)
            {
                page.AppendLine("BT");
                page.AppendLine("/F1 10 Tf");
                page.AppendLine("50 750 Td");
                page.AppendLine("14 TL");
            }

            page.AppendLine($"({Escape(line)}) Tj");
            page.AppendLine("T*");
            lineOnPage++;

            if (lineOnPage >= 48)
            {
                page.AppendLine("ET");
                pages.Add(page.ToString());
                page.Clear();
                lineOnPage = 0;
            }
        }

        if (lineOnPage > 0)
        {
            page.AppendLine("ET");
            pages.Add(page.ToString());
        }

        return pages.Count == 0 ? ["BT /F1 10 Tf 50 750 Td (No report data.) Tj ET"] : pages;
    }

    private static IEnumerable<string> WrapLine(string line)
    {
        const int maxLength = 92;

        if (line.Length <= maxLength)
        {
            yield return line;
            yield break;
        }

        string remaining = line;

        while (remaining.Length > maxLength)
        {
            int splitAt = remaining.LastIndexOf(' ', maxLength);
            splitAt = splitAt <= 0 ? maxLength : splitAt;
            yield return remaining[..splitAt];
            remaining = remaining[splitAt..].TrimStart();
        }

        if (remaining.Length > 0)
        {
            yield return remaining;
        }
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
