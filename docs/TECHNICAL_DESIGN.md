# Technical Design

## Stack Decision

The Windows app uses C# with .NET 8, WPF, and focused Win32 interop.

This stack is chosen because Easydict is not a document-style app. Its most important behaviors are desktop integration behaviors:

- Global shortcuts
- Non-activating popup display
- Tray operation
- Foreground app preservation
- Clipboard and selected text access
- Multi-monitor screenshots
- OCR handoff

WPF provides a mature managed UI layer while still allowing direct access to HWND handles and Win32 messages. This makes it a practical first choice for a native Windows Easydict client.

## High-Level Architecture

```text
UI Layer
  MainWindow
  QueryPopupWindow
  Future settings and tray windows

Application Services
  Hotkey service
  Popup coordinator
  Lookup workflow
  Screenshot workflow

Platform Services
  Win32 hotkeys
  Win32 non-activating window styles
  Screen capture
  Clipboard and selected text acquisition
  Windows OCR

Domain Services
  Translation providers
  OCR provider abstraction
  Result models
  Configuration
```

## Current Code Structure

```text
Easydict.Windows/
  App.xaml
  MainWindow.xaml
  MainWindow.xaml.cs
  app.manifest
  Services/
    Hotkeys/
      GlobalHotkeyService.cs
    Interop/
      WindowInterop.cs
    Ocr/
      IOcrService.cs
      WindowsOcrService.cs
    ScreenCapture/
      ScreenCaptureService.cs
    Translation/
      ITranslator.cs
      EchoTranslator.cs
  Views/
    QueryPopupWindow.xaml
    QueryPopupWindow.xaml.cs
```

## Key Components

### GlobalHotkeyService

`GlobalHotkeyService` registers global shortcuts with `RegisterHotKey` and listens for `WM_HOTKEY` through an `HwndSource` hook.

The service should remain small and platform-specific. Higher-level workflow decisions should live outside it.

Current actions:

- `Lookup`
- `Ocr`

Future work:

- Configurable hotkeys
- Conflict reporting
- Temporary enable/disable from tray menu

### QueryPopupWindow

`QueryPopupWindow` is the first lookup surface. It is designed to appear near the cursor and avoid stealing focus.

It uses:

- `ShowActivated="False"`
- `ShowWithoutActivation`
- `WS_EX_NOACTIVATE`
- `WS_EX_TOOLWINDOW`
- `ShowWindow(..., SW_SHOWNOACTIVATE)`

The popup should eventually be coordinated by a dedicated popup service so multiple workflows can reuse the same behavior.

### ScreenCaptureService

`ScreenCaptureService` currently captures the virtual bounds of all screens with `Graphics.CopyFromScreen`.

Future work:

- Region selection overlay
- Per-monitor DPI correctness checks
- Exclude Easydict popup windows from capture when possible
- Support delayed capture for shortcut flows

### WindowsOcrService

`WindowsOcrService` wraps Windows Runtime OCR. It receives a WPF `BitmapSource`, encodes it as PNG, decodes it into a WinRT `SoftwareBitmap`, and runs OCR with `OcrEngine.TryCreateFromUserProfileLanguages()`.

Future work:

- OCR language selection
- Better image preprocessing
- Cancellation-aware capture workflows
- Optional external OCR providers

### Translation Services

`ITranslator` is intentionally minimal right now:

```csharp
Task<TranslationResult> TranslateAsync(string text, CancellationToken cancellationToken);
```

Future work should expand this into a provider-based model similar to the macOS app:

- Provider identity
- Source and target language
- Error metadata
- Partial results
- Streaming results for AI providers
- Secure API key storage

The current custom HTTP provider supports JSON or form POST bodies, optional `Authorization`, configurable query field names, and dot-path result extraction. Result paths can include array indexes, such as `choices.0.message.content`. The built-in question bank preset uses:

```text
URL: http://47.109.40.237:5000/api/v1/search
Body: JSON
Query field: keyword
Result path: data.success
```

Results containing `question`, `options`, and `answer` fields are rendered as readable question records.

## Selected Text Acquisition

Windows selected text lookup should use a layered strategy:

1. Try UI Automation text patterns for the currently focused element.
2. Preserve the current clipboard content.
3. Send `Ctrl+C` to the foreground app.
4. Read copied text if it changes quickly.
5. Restore the previous clipboard content.

This must be implemented carefully to avoid corrupting the user's clipboard.

## No-Focus Behavior

The lookup popup must not steal focus from the current foreground app when invoked by a shortcut.

Important rules:

- Do not call `Activate()` for lookup popups.
- Prefer `ShowWindow` with `SW_SHOWNOACTIVATE`.
- Apply `WS_EX_NOACTIVATE` before showing the popup.
- Keep the popup out of Alt-Tab with `WS_EX_TOOLWINDOW`.
- Only focus text input when the user explicitly opens an interactive manual mode.

## Configuration

Early configuration can use a JSON file under:

```text
%APPDATA%\Easydict\settings.json
```

Sensitive values should later move to Windows Credential Manager or DPAPI-protected storage.

The current implementation persists settings with `SettingsService` and stores:

- Hotkey gestures
- Custom HTTP provider configuration
- OCR preferences

Suggested settings model:

- Hotkeys
- Enabled services
- OCR language
- Source and target language preferences
- Popup placement
- Startup behavior
- Provider credentials references

## Packaging

Recommended packaging path:

1. Plain `dotnet publish` for internal testing.
2. MSIX for signed distribution.
3. Optional installer wrapper if MSIX restrictions become a problem.

## Diagnostics

The app uses a single-instance mutex to avoid duplicate tray instances. Unhandled exceptions are written to:

```text
%APPDATA%\Easydict\logs
```

## History

Query history is stored as JSON under:

```text
%APPDATA%\Easydict\history.json
```

The history service keeps the newest 200 records and stores query text, result text, source, and timestamp.

## Startup Registration

Launch-at-login uses the current user's Run key:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

This does not require administrator privileges and can be toggled from the main window.

## Testing Strategy

Unit tests:

- Settings serialization
- Translation provider request construction
- Result parsing
- Hotkey config parsing

Manual and integration tests:

- Hotkey registration conflicts
- No-focus popup behavior
- Multi-monitor screenshot capture
- DPI scaling
- OCR result quality
- Clipboard preservation

## Known Risks

- Windows OCR availability varies with installed language packs.
- Clipboard-based selected text lookup can interfere with user clipboard state if implemented carelessly.
- Global hotkeys may conflict with other software.
- Multi-monitor DPI behavior needs real Windows hardware validation.
- WPF cannot be fully validated from macOS.
