using System.Windows;
using Easydict.Windows.Models;
using Easydict.Windows.Services.Clipboard;
using Easydict.Windows.Services.Configuration;
using Easydict.Windows.Services.Diagnostics;
using Easydict.Windows.Services.Hotkeys;
using Easydict.Windows.Services.History;
using Easydict.Windows.Services.Ocr;
using Easydict.Windows.Services.ScreenCapture;
using Easydict.Windows.Services.Startup;
using Easydict.Windows.Services.Tray;
using Easydict.Windows.Services.Translation;
using Easydict.Windows.Views;
using WpfApplication = System.Windows.Application;
using WpfClipboard = System.Windows.Clipboard;

namespace Easydict.Windows;

public partial class MainWindow : Window
{
    private readonly SettingsService settingsService = new();
    private readonly StartupService startupService = new();
    private readonly StartupRegistrationService startupRegistrationService = new();
    private readonly HistoryService historyService = new();
    private readonly IOcrService ocrService = new WindowsOcrService();
    private readonly ScreenCaptureService screenCaptureService = new();
    private readonly SelectedTextService selectedTextService = new();
    private GlobalHotkeyService? hotkeyService;
    private TrayIconService? trayIconService;
    private QueryPopupWindow? queryPopupWindow;
    private HistoryWindow? historyWindow;
    private AppSettings settings = AppSettings.Default;
    private ITranslator translator = new EchoTranslator();
    private bool isExitRequested;

    public bool ShouldStartInTray => settings.StartInTray;

    public MainWindow()
    {
        settings = settingsService.Load();
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ApplySettingsToForm();
        RebuildTranslator();

        trayIconService = new TrayIconService();
        trayIconService.ShowRequested += (_, _) => ShowMainWindow();
        trayIconService.InputRequested += (_, _) => OpenLookupPopup(activateForInput: true);
        trayIconService.LookupRequested += (_, _) => _ = OpenLookupFromSelectionAsync();
        trayIconService.OcrRequested += (_, _) => _ = CaptureOcrAsync();
        trayIconService.RegionOcrRequested += (_, _) => _ = CaptureRegionOcrAsync();
        trayIconService.SilentOcrRequested += (_, _) => _ = CaptureSilentRegionOcrAsync();
        trayIconService.HistoryRequested += (_, _) => ShowHistoryWindow();
        trayIconService.ExitRequested += (_, _) => ExitApplication();
        trayIconService.Show();
        SetStatus(string.Join(" | ", startupService.GetEnvironmentSummary().Take(2)));

        if (settings.StartInTray)
        {
            HideToTray(showNotification: true);
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        try
        {
            RegisterHotkeys();
        }
        catch (Exception ex)
        {
            AppLogger.Log(ex, "Initial hotkey registration failed.");
            SetStatus($"全局快捷键注册失败：{ex.Message}");
        }
    }

    private void HotkeyService_HotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        if (e.Action == HotkeyAction.Input)
        {
            OpenLookupPopup(activateForInput: true);
            return;
        }

        if (e.Action == HotkeyAction.Lookup)
        {
            _ = OpenLookupFromSelectionAsync();
            return;
        }

        if (e.Action == HotkeyAction.Ocr)
        {
            _ = CaptureOcrAsync();
            return;
        }

        if (e.Action == HotkeyAction.SilentOcr)
        {
            _ = CaptureSilentRegionOcrAsync();
        }
    }

    private void OpenLookupButton_Click(object sender, RoutedEventArgs e)
    {
        OpenLookupPopup(activateForInput: true);
    }

    private void CaptureOcrButton_Click(object sender, RoutedEventArgs e)
    {
        _ = CaptureOcrAsync();
    }

    private void RegionOcrButton_Click(object sender, RoutedEventArgs e)
    {
        _ = CaptureRegionOcrAsync();
    }

    private void SilentOcrButton_Click(object sender, RoutedEventArgs e)
    {
        _ = CaptureSilentRegionOcrAsync();
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ShowHistoryWindow();
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var hotkeysReplaced = false;
        try
        {
            var nextSettings = CreateSettingsFromForm();
            var nextTranslator = BuildTranslator(nextSettings);
            ValidateHotkeyUniqueness(nextSettings);
            ReplaceHotkeys(nextSettings);
            hotkeysReplaced = true;
            startupRegistrationService.SetEnabled(nextSettings.LaunchAtLogin);
            settingsService.Save(nextSettings);

            settings = nextSettings;
            translator = nextTranslator;
            queryPopupWindow?.SetTranslator(translator);
            SetStatus("设置已保存");
        }
        catch (Exception ex)
        {
            if (hotkeysReplaced)
            {
                TryRestoreCurrentHotkeys();
            }

            AppLogger.Log(ex, "Settings save failed.");
            try
            {
                startupRegistrationService.SetEnabled(settings.LaunchAtLogin);
            }
            catch (Exception rollbackException)
            {
                AppLogger.Log(rollbackException, "Startup registration rollback failed.");
            }

            ApplySettingsToForm();
            queryPopupWindow?.SetTranslator(translator);
            SetStatus($"保存设置失败：{ex.Message}");
        }
    }

    private async void TestServiceButton_Click(object sender, RoutedEventArgs e)
    {
        var previousTranslator = translator;
        try
        {
            translator = BuildTranslator(CreateTranslationSettingsFromForm());
            queryPopupWindow?.SetTranslator(translator);
            OpenLookupPopup(activateForInput: true);
            if (queryPopupWindow is not null)
            {
                await queryPopupWindow.SearchTextAsync("题干关键词");
            }
        }
        finally
        {
            translator = previousTranslator;
            queryPopupWindow?.SetTranslator(translator);
        }
    }

    private void ApplyQuestionPresetButton_Click(object sender, RoutedEventArgs e)
    {
        ApiUrlTextBox.Text = "http://47.109.40.237:5000/api/v1/search";
        JsonBodyCheckBox.IsChecked = true;
        QueryFieldTextBox.Text = "keyword";
        ResultPathTextBox.Text = "data.success";
        SetStatus("题库接口预设已填入，请补充 Authorization 后保存");
    }

    private void OpenLookupPopup(string? presetText = null, bool activateForInput = false)
    {
        queryPopupWindow ??= new QueryPopupWindow(translator);
        queryPopupWindow.SetTranslator(translator);
        queryPopupWindow.QueryCompleted -= QueryPopupWindow_QueryCompleted;
        queryPopupWindow.QueryCompleted += QueryPopupWindow_QueryCompleted;
        queryPopupWindow.ShowNearCursor(presetText, activateForInput);
        SetStatus("查询浮窗已显示");
    }

    private void QueryPopupWindow_QueryCompleted(object? sender, QueryHistoryItem e)
    {
        historyService.Add(e);
        historyWindow?.RefreshHistory();
    }

    private async Task OpenLookupFromSelectionAsync()
    {
        try
        {
            SetStatus("正在读取选中文字...");
            var selectedText = await selectedTextService.TryReadSelectedTextAsync(TimeSpan.FromMilliseconds(180), CancellationToken.None);
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                OpenLookupPopup(activateForInput: true);
                SetStatus("未读取到选中文字");
                return;
            }

            OpenLookupPopup(selectedText);
            SetStatus("已读取选中文字");
        }
        catch (Exception ex)
        {
            AppLogger.Log(ex, "Selected text lookup failed.");
            OpenLookupPopup(activateForInput: true);
            SetStatus($"读取选中文字失败：{ex.Message}");
        }
    }

    private async Task CaptureOcrAsync()
    {
        var shouldRestoreMainWindow = ShouldRestoreMainWindowAfterCapture();
        try
        {
            SetStatus("正在截图并识别文字...");
            Hide();
            await Task.Delay(160);
            var image = screenCaptureService.CaptureAllScreens();
            var recognizedText = await ocrService.RecognizeAsync(image, CancellationToken.None);
            RestoreMainWindowAfterCapture(shouldRestoreMainWindow);
            OpenLookupPopup(recognizedText);
            SetStatus(string.IsNullOrWhiteSpace(recognizedText) ? "OCR 未识别到文字" : "OCR 已完成");
        }
        catch (Exception ex)
        {
            AppLogger.Log(ex, "Full-screen OCR failed.");
            RestoreMainWindowAfterCapture(shouldRestoreMainWindow);
            SetStatus($"OCR 失败：{ex.Message}");
        }
    }

    private async Task CaptureRegionOcrAsync()
    {
        var shouldRestoreMainWindow = ShouldRestoreMainWindowAfterCapture();
        try
        {
            SetStatus("请选择截图区域...");
            var recognizedText = await CaptureSelectedRegionTextAsync(shouldRestoreMainWindow);
            if (recognizedText is null)
            {
                SetStatus("已取消区域截图");
                return;
            }

            OpenLookupPopup(recognizedText);
            SetStatus(string.IsNullOrWhiteSpace(recognizedText) ? "OCR 未识别到文字" : "区域 OCR 已完成");
        }
        catch (Exception ex)
        {
            AppLogger.Log(ex, "Region OCR failed.");
            RestoreMainWindowAfterCapture(shouldRestoreMainWindow);
            SetStatus($"区域 OCR 失败：{ex.Message}");
        }
    }

    private async Task CaptureSilentRegionOcrAsync()
    {
        var shouldRestoreMainWindow = ShouldRestoreMainWindowAfterCapture();
        try
        {
            SetStatus("请选择静默 OCR 区域...");
            var recognizedText = await CaptureSelectedRegionTextAsync(shouldRestoreMainWindow);
            if (recognizedText is null)
            {
                SetStatus("已取消静默 OCR");
                return;
            }

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                SetStatus("静默 OCR 未识别到文字");
                return;
            }

            WpfClipboard.SetText(recognizedText);
            SetStatus("静默 OCR 已复制到剪贴板");
        }
        catch (Exception ex)
        {
            AppLogger.Log(ex, "Silent region OCR failed.");
            RestoreMainWindowAfterCapture(shouldRestoreMainWindow);
            SetStatus($"静默 OCR 失败：{ex.Message}");
        }
    }

    private async Task<string?> CaptureSelectedRegionTextAsync(bool restoreMainWindow)
    {
        Hide();
        await Task.Delay(120);

        var selectionWindow = new RegionSelectionWindow();
        var region = selectionWindow.SelectRegion();
        if (region is null || region.Value.Width <= 2 || region.Value.Height <= 2)
        {
            if (restoreMainWindow)
            {
                ShowMainWindow();
            }

            return null;
        }

        SetStatus("正在识别区域文字...");
        await Task.Delay(180);
        var image = screenCaptureService.CaptureRegion(region.Value);
        var text = await ocrService.RecognizeAsync(image, CancellationToken.None);
        if (restoreMainWindow)
        {
            ShowMainWindow();
        }

        return text;
    }

    private void ApplySettingsToForm()
    {
        ApiUrlTextBox.Text = settings.Translation.ApiUrl;
        AuthTokenBox.Password = settings.Translation.AuthToken;
        QueryFieldTextBox.Text = settings.Translation.QueryField;
        JsonBodyCheckBox.IsChecked = settings.Translation.RequestBodyType == RequestBodyType.Json;
        ResultPathTextBox.Text = settings.Translation.ResultPath;
        SourceLanguageTextBox.Text = settings.Translation.SourceLanguage;
        TargetLanguageTextBox.Text = settings.Translation.TargetLanguage;
        InputHotkeyTextBox.Text = HotkeyGestureParser.ToDisplayText(settings.Hotkeys.Input);
        LookupHotkeyTextBox.Text = HotkeyGestureParser.ToDisplayText(settings.Hotkeys.Lookup);
        OcrHotkeyTextBox.Text = HotkeyGestureParser.ToDisplayText(settings.Hotkeys.Ocr);
        SilentOcrHotkeyTextBox.Text = HotkeyGestureParser.ToDisplayText(settings.Hotkeys.SilentOcr);
        LaunchAtLoginCheckBox.IsChecked = settings.LaunchAtLogin;
        StartInTrayCheckBox.IsChecked = settings.StartInTray;
        MinimizeToTrayCheckBox.IsChecked = settings.MinimizeToTray;
    }

    private AppSettings CreateSettingsFromForm()
    {
        return new AppSettings
        {
            Translation = CreateTranslationSettingsFromForm(),
            Hotkeys = new HotkeySettings
            {
                Input = HotkeyGestureParser.Parse(InputHotkeyTextBox.Text),
                Lookup = HotkeyGestureParser.Parse(LookupHotkeyTextBox.Text),
                Ocr = HotkeyGestureParser.Parse(OcrHotkeyTextBox.Text),
                SilentOcr = HotkeyGestureParser.Parse(SilentOcrHotkeyTextBox.Text),
            },
            Ocr = new OcrSettings
            {
                LanguageTag = settings.Ocr.LanguageTag,
            },
            LaunchAtLogin = LaunchAtLoginCheckBox.IsChecked == true,
            StartInTray = StartInTrayCheckBox.IsChecked == true,
            MinimizeToTray = MinimizeToTrayCheckBox.IsChecked == true,
        };
    }

    private TranslationSettings CreateTranslationSettingsFromForm()
    {
        return new TranslationSettings
        {
            ApiUrl = ApiUrlTextBox.Text.Trim(),
            AuthToken = AuthTokenBox.Password.Trim(),
            QueryField = string.IsNullOrWhiteSpace(QueryFieldTextBox.Text) ? "text" : QueryFieldTextBox.Text.Trim(),
            RequestBodyType = JsonBodyCheckBox.IsChecked == true ? RequestBodyType.Json : RequestBodyType.Form,
            ResultPath = ResultPathTextBox.Text.Trim(),
            SourceLanguage = string.IsNullOrWhiteSpace(SourceLanguageTextBox.Text) ? "auto" : SourceLanguageTextBox.Text.Trim(),
            TargetLanguage = string.IsNullOrWhiteSpace(TargetLanguageTextBox.Text) ? "zh" : TargetLanguageTextBox.Text.Trim(),
        };
    }

    private void RebuildTranslator()
    {
        translator = BuildTranslator(settings);
    }

    private static ITranslator BuildTranslator(AppSettings targetSettings)
    {
        return BuildTranslator(targetSettings.Translation);
    }

    private static ITranslator BuildTranslator(TranslationSettings targetSettings)
    {
        return string.IsNullOrWhiteSpace(targetSettings.ApiUrl)
            ? new EchoTranslator()
            : new CustomHttpTranslator(targetSettings);
    }

    private void RegisterHotkeys()
    {
        ReplaceHotkeys(settings);
        SetStatus("全局快捷键已注册");
    }

    private void ReplaceHotkeys(AppSettings targetSettings)
    {
        if (hotkeyService is not null && AreHotkeysEqual(settings, targetSettings))
        {
            return;
        }

        var previousHotkeyService = hotkeyService;
        hotkeyService = null;
        previousHotkeyService?.Dispose();

        try
        {
            hotkeyService = CreateHotkeyService(targetSettings);
        }
        catch
        {
            TryRestoreCurrentHotkeys();
            throw;
        }
    }

    private void TryRestoreCurrentHotkeys()
    {
        hotkeyService?.Dispose();
        hotkeyService = null;

        try
        {
            hotkeyService = CreateHotkeyService(settings);
        }
        catch (Exception restoreException)
        {
            AppLogger.Log(restoreException, "Hotkey rollback failed.");
        }
    }

    private GlobalHotkeyService CreateHotkeyService(AppSettings targetSettings)
    {
        ValidateHotkeyUniqueness(targetSettings);

        var nextHotkeyService = new GlobalHotkeyService(this);
        try
        {
            nextHotkeyService.HotkeyPressed += HotkeyService_HotkeyPressed;
            nextHotkeyService.Register(HotkeyAction.Input, targetSettings.Hotkeys.Input);
            nextHotkeyService.Register(HotkeyAction.Lookup, targetSettings.Hotkeys.Lookup);
            nextHotkeyService.Register(HotkeyAction.Ocr, targetSettings.Hotkeys.Ocr);
            nextHotkeyService.Register(HotkeyAction.SilentOcr, targetSettings.Hotkeys.SilentOcr);
        }
        catch
        {
            nextHotkeyService.Dispose();
            throw;
        }

        return nextHotkeyService;
    }

    private static void ValidateHotkeyUniqueness(AppSettings targetSettings)
    {
        var hotkeys = new[]
        {
            ("输入查询", targetSettings.Hotkeys.Input),
            ("划词查询", targetSettings.Hotkeys.Lookup),
            ("OCR", targetSettings.Hotkeys.Ocr),
            ("静默 OCR", targetSettings.Hotkeys.SilentOcr),
        };

        var duplicate = hotkeys
            .GroupBy(item => HotkeyGestureParser.ToInvariantText(item.Item2))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"快捷键冲突：{string.Join("、", duplicate.Select(item => item.Item1))}");
        }
    }

    private static bool AreHotkeysEqual(AppSettings first, AppSettings second)
    {
        return HotkeyGestureParser.ToInvariantText(first.Hotkeys.Input) == HotkeyGestureParser.ToInvariantText(second.Hotkeys.Input)
            && HotkeyGestureParser.ToInvariantText(first.Hotkeys.Lookup) == HotkeyGestureParser.ToInvariantText(second.Hotkeys.Lookup)
            && HotkeyGestureParser.ToInvariantText(first.Hotkeys.Ocr) == HotkeyGestureParser.ToInvariantText(second.Hotkeys.Ocr)
            && HotkeyGestureParser.ToInvariantText(first.Hotkeys.SilentOcr) == HotkeyGestureParser.ToInvariantText(second.Hotkeys.SilentOcr);
    }

    private void ShowHistoryWindow()
    {
        if (historyWindow is null)
        {
            historyWindow = new HistoryWindow(historyService);
            historyWindow.Closed += (_, _) => historyWindow = null;
        }

        historyWindow.RefreshHistory();
        historyWindow.Show();
        historyWindow.Activate();
    }

    private void SetStatus(string message)
    {
        StatusText.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && settings.MinimizeToTray && !isExitRequested)
        {
            HideToTray(showNotification: false);
        }
    }

    private void HideToTray(bool showNotification)
    {
        ShowInTaskbar = false;
        Hide();
        SetStatus("已隐藏到系统托盘");
        if (showNotification)
        {
            trayIconService?.ShowInfo("Easydict", "Easydict is running in the system tray.");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        hotkeyService?.Dispose();
        trayIconService?.Dispose();
        queryPopupWindow?.Close();
        historyWindow?.Close();
        base.OnClosed(e);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!isExitRequested)
        {
            e.Cancel = true;
            HideToTray(showNotification: true);
            return;
        }

        base.OnClosing(e);
    }

    private void ShowMainWindow()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private bool ShouldRestoreMainWindowAfterCapture()
    {
        return IsVisible && WindowState != WindowState.Minimized;
    }

    private void RestoreMainWindowAfterCapture(bool shouldRestoreMainWindow)
    {
        if (shouldRestoreMainWindow)
        {
            ShowMainWindow();
        }
        else
        {
            HideToTray(showNotification: false);
        }
    }

    public void RequestShowMainWindow()
    {
        ShowMainWindow();
    }

    private void ExitApplication()
    {
        isExitRequested = true;
        Close();
        WpfApplication.Current.Shutdown();
    }
}
