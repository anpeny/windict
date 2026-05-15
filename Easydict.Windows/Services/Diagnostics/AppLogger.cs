namespace Easydict.Windows.Services.Diagnostics;

public static class AppLogger
{
    public static void Log(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var path = Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
            File.AppendAllText(
                path,
                $"[{DateTime.Now:O}] {exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never crash the application.
        }
    }

    private static string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Easydict", "logs");
}
