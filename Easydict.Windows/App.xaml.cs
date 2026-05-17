using System.Windows;
using Easydict.Windows.Services.Diagnostics;
using WpfApplication = System.Windows.Application;
using WpfExitEventArgs = System.Windows.ExitEventArgs;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfStartupEventArgs = System.Windows.StartupEventArgs;
using WpfWindowState = System.Windows.WindowState;

namespace Easydict.Windows;

public partial class App : WpfApplication
{
    private const string SingleInstanceMutexName = "Easydict.Windows.SingleInstance";
    private const string ActivationEventName = "Easydict.Windows.Activate";

    private Mutex? singleInstanceMutex;
    private EventWaitHandle? activationEvent;
    private CancellationTokenSource? activationListenerCancellation;
    private bool ownsSingleInstanceMutex;
    private MainWindow? mainWindow;

    protected override void OnStartup(WpfStartupEventArgs e)
    {
        singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            SignalExistingInstance();
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
        StartActivationListener();
        ShowMainWindow();
    }

    protected override void OnExit(WpfExitEventArgs e)
    {
        activationListenerCancellation?.Cancel();
        activationEvent?.Set();
        activationListenerCancellation?.Dispose();
        activationEvent?.Dispose();

        if (ownsSingleInstanceMutex)
        {
            singleInstanceMutex?.ReleaseMutex();
        }

        singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private static void SignalExistingInstance()
    {
        try
        {
            using var existingActivationEvent = EventWaitHandle.OpenExisting(ActivationEventName);
            existingActivationEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }

    private void StartActivationListener()
    {
        activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);
        activationListenerCancellation = new CancellationTokenSource();
        var cancellationToken = activationListenerCancellation.Token;

        Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                activationEvent.WaitOne();
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Dispatcher.BeginInvoke(() => mainWindow?.RequestShowMainWindow());
            }
        }, cancellationToken);
    }

    private void ShowMainWindow()
    {
        mainWindow = new MainWindow();
        if (mainWindow.ShouldStartInTray)
        {
            mainWindow.ShowActivated = false;
            mainWindow.ShowInTaskbar = false;
            mainWindow.WindowState = WpfWindowState.Minimized;
        }

        mainWindow.Show();
    }
}
