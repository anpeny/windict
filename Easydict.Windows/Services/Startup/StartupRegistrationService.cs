using Microsoft.Win32;

namespace Easydict.Windows.Services.Startup;

public sealed class StartupRegistrationService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Easydict";

    public void SetEnabled(bool isEnabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (isEnabled)
        {
            key.SetValue(AppName, $"\"{Environment.ProcessPath}\"");
            return;
        }

        key.DeleteValue(AppName, throwOnMissingValue: false);
    }
}
