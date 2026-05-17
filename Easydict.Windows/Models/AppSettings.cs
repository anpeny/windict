using Easydict.Windows.Services.Hotkeys;

namespace Easydict.Windows.Models;

public sealed class AppSettings
{
    public HotkeySettings Hotkeys { get; set; } = new();
    public TranslationSettings Translation { get; set; } = new();
    public OcrSettings Ocr { get; set; } = new();
    public bool LaunchAtLogin { get; set; }

    public static AppSettings Default => new();
}

public sealed class HotkeySettings
{
    public HotkeyGesture Input { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
        VirtualKey = 0x41,
    };

    public HotkeyGesture Lookup { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
        VirtualKey = 0x44,
    };

    public HotkeyGesture Ocr { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.NoRepeat,
        VirtualKey = 0x53,
    };

    public HotkeyGesture SilentOcr { get; set; } = new()
    {
        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.NoRepeat,
        VirtualKey = 0x53,
    };
}

public sealed class TranslationSettings
{
    public string ApiUrl { get; set; } = "http://47.109.40.237:5000/api/v1/search";
    public string AuthToken { get; set; } = string.Empty;
    public RequestBodyType RequestBodyType { get; set; } = RequestBodyType.Json;
    public string QueryField { get; set; } = "keyword";
    public string SourceLanguage { get; set; } = "auto";
    public string TargetLanguage { get; set; } = "zh";
    public string ResultPath { get; set; } = "data.success";
}

public sealed class OcrSettings
{
    public string LanguageTag { get; set; } = string.Empty;
}

public enum RequestBodyType
{
    Form,
    Json,
}
