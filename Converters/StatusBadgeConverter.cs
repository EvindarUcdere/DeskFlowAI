using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DeskFlowAI.Converters;

public sealed class StatusBadgeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value?.ToString() ?? string.Empty;
        string mode = parameter?.ToString() ?? "Text";
        BadgeStyle style = GetStyle(status);

        return mode switch
        {
            "Background" => new SolidColorBrush(style.Background),
            "Foreground" => new SolidColorBrush(style.Foreground),
            "Border" => new SolidColorBrush(style.Border),
            _ => $"{style.Dot} {status}"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static BadgeStyle GetStyle(string status)
    {
        return status switch
        {
            "Active" or "Available" or "Approved" or "Done" or "Analyzed" or "External AI Allowed" or "Low" or "Ready" or "Extracted" => new("\u25CF", "#EAF8F0", "#166534", "#BBF7D0"),
            "Planning" or "To Do" or "Uploaded" or "Not Analyzed" or "Normal" or "Internal Only" or "Not Checked" or "Not Extracted" => new("\u25CF", "#EEF4FF", "#1D4ED8", "#BFDBFE"),
            "In Progress" or "In Review" or "Busy" or "Pending" or "Medium" or "Needs Approval" or "File Not Ready" => new("\u25CF", "#FFF7ED", "#9A3412", "#FED7AA"),
            "Review" => new("\u25CF", "#F5F3FF", "#6D28D9", "#DDD6FE"),
            "On Hold" or "Blocked" or "Needs Update" or "Critical" or "Failed" or "High" or "File Missing" or "Unsupported File" or "Read Error" or "Extract Error" => new("\u25CF", "#FEF2F2", "#991B1B", "#FECACA"),
            "Completed" or "On Leave" or "Emergency Cover" => new("\u25CF", "#F5F3FF", "#6D28D9", "#DDD6FE"),
            _ => new("\u25CF", "#F8FAFC", "#475569", "#E2E8F0")
        };
    }

    private sealed record BadgeStyle(string Dot, string BackgroundHex, string ForegroundHex, string BorderHex)
    {
        public Color Background => (Color)ColorConverter.ConvertFromString(BackgroundHex);

        public Color Foreground => (Color)ColorConverter.ConvertFromString(ForegroundHex);

        public Color Border => (Color)ColorConverter.ConvertFromString(BorderHex);
    }
}
