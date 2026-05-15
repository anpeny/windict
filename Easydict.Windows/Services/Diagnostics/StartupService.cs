using System.Runtime.InteropServices;

namespace Easydict.Windows.Services.Diagnostics;

public sealed class StartupService
{
    public IReadOnlyList<string> GetEnvironmentSummary()
    {
        var values = new List<string>
        {
            $"OS: {RuntimeInformation.OSDescription}",
            $".NET: {Environment.Version}",
            $"Machine: {Environment.MachineName}",
            $"User: {Environment.UserName}",
        };

        return values;
    }
}
