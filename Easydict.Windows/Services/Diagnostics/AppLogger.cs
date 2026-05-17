using System.IO;
using System.Text.RegularExpressions;

namespace Easydict.Windows.Services.Diagnostics;

public static class AppLogger
{
    public static void Log(Exception exception, string? context = null)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var path = Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
            var contextPrefix = string.IsNullOrWhiteSpace(context) ? string.Empty : $"{context}{Environment.NewLine}";
            File.AppendAllText(
                path,
                $"[{DateTime.Now:O}] {contextPrefix}{Sanitize(exception.ToString())}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never crash the application.
        }
    }

    private static string Sanitize(string text)
    {
        return SensitiveValuePattern.Replace(text, "$1 <redacted>");
    }

    private static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Easydict", "logs");

    private static readonly Regex SensitiveValuePattern = new(
        @"(?i)\b(authorization|token|api[-_ ]?key|password)\b\s*[:=]\s*[^\r\n]+",
        RegexOptions.Compiled);
}
