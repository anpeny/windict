using System.Windows;
using Easydict.Windows.Services.Diagnostics;
using WpfApplication = System.Windows.Application;
using WpfExitEventArgs = System.Windows.ExitEventArgs;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfStartupEventArgs = System.Windows.StartupEventArgs;

namespace Easydict.Windows;

public partial class App : WpfApplication
{
    private Mutex? singleInstanceMutex;
    private bool ownsSingleInstanceMutex;

    protected override void OnStartup(WpfStartupEventArgs e)
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
            WpfMessageBox.Show(args.Exception.Message, "Easydict", WpfMessageBoxButton.OK, WpfMessageBoxImage.Error);
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

    protected override void OnExit(WpfExitEventArgs e)
    {
        if (ownsSingleInstanceMutex)
        {
            singleInstanceMutex?.ReleaseMutex();
        }

        singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
