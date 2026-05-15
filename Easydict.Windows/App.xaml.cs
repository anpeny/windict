using System.Windows;
using Easydict.Windows.Services.Diagnostics;

namespace Easydict.Windows;

public partial class App : Application
{
    private Mutex? singleInstanceMutex;
    private bool ownsSingleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        singleInstanceMutex = new Mutex(true, "Easydict.Windows.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        ownsSingleInstanceMutex = true;
        DispatcherUnhandledException += (_, args) =>
        {
            AppLogger.Log(args.Exception);
            MessageBox.Show(args.Exception.Message, "Easydict", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                AppLogger.Log(exception);
            }
        };

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ownsSingleInstanceMutex)
        {
            singleInstanceMutex?.ReleaseMutex();
        }

        singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
