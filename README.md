# Easydict for Windows

Easydict for Windows is a native Windows dictionary, translation, and OCR lookup app. It aims to preserve the core Easydict workflow from macOS while using Windows-native desktop technology for shortcuts, screen capture, OCR, popup display, and background operation.

This repository is intentionally separate from the macOS project. The two clients should share product thinking and service behavior, but they should not be forced into the same platform architecture.

## Current Status

The project currently contains a first runnable skeleton:

- Main WPF control window
- Global hotkeys
- Non-activating query popup window
- Full-screen capture service
- Region screenshot selection
- Windows OCR service wrapper
- Tray background operation
- Selected text lookup through clipboard preservation
- Persistent settings under `%APPDATA%\Easydict\settings.json`
- Custom HTTP translation provider with JSON/Form request support
- Single-instance startup guard
- Local crash logs under `%APPDATA%\Easydict\logs`
- Optional current-user launch at login
- Query history under `%APPDATA%\Easydict\history.json`
- Tray menu actions for lookup, OCR, region OCR, and history

## Technology Stack

- **Language/runtime:** C# with .NET 8
- **UI:** WPF
- **Native integration:** Win32 interop
- **OCR:** Windows Runtime OCR (`Windows.Media.Ocr`)
- **Packaging target:** MSIX or standalone installer in a later milestone

WPF is selected for the first Windows version because Easydict depends on classic desktop behaviors that are still easiest to implement reliably with WPF and Win32:

- Registering global hotkeys
- Showing popup windows without stealing focus
- Placing floating windows near the cursor
- Capturing screens across monitors
- Running as a tray app
- Interoperating with clipboard and foreground-window APIs

WinUI 3 can be reconsidered later for a larger UI rewrite, but it adds friction for the low-level desktop behaviors that matter most in the first version.

## Project Layout

```text
Easydict/
  AGENTS.md
  README.md
  docs/
    PRODUCT_SPEC.md
    TECHNICAL_DESIGN.md
    IMPLEMENTATION_PLAN.md
  Easydict.Windows/
    App.xaml
    MainWindow.xaml
    Services/
    Views/
```

## Development Requirements

- Windows 10 2004 or newer
- .NET 8 SDK
- Visual Studio 2022 or newer with the desktop development workload

## Continuous Integration

GitHub Actions builds the WPF app on `windows-latest`, publishes a self-contained `win-x64` folder artifact, and validates project XML on Ubuntu.

Workflow:

```text
.github/workflows/windows-build.yml
```

Build from Windows:

```powershell
dotnet build .\Easydict.Windows\Easydict.Windows.csproj
```

Run from Windows:

```powershell
dotnet run --project .\Easydict.Windows\Easydict.Windows.csproj
```

Scripted build:

```powershell
.\scripts\build.ps1 -Configuration Release
```

Publish a self-contained x64 folder build:

```powershell
.\scripts\publish-win-x64.ps1
```

## Initial Shortcuts

- `Ctrl+Alt+D`: show lookup popup
- `Ctrl+Alt+S`: capture screen and run OCR

These defaults can be changed in the main window with shortcut strings such as `Ctrl+Alt+D`, `Ctrl+Shift+Space`, or `Alt+Q`.

## Custom Translation Provider

The first provider is a generic HTTP POST provider. It supports:

- JSON or form request bodies
- Custom query field name
- `from` and `to` language fields
- Optional `Authorization` header
- Dot-path JSON result extraction, such as `data.translation`
- Array index result paths, such as `choices.0.message.content`
- Friendly rendering for `question`, `options`, and `answer` search results
- One-click preset for `http://47.109.40.237:5000/api/v1/search`

If no API URL is configured, the app uses a local placeholder translator so the lookup UI remains testable.

## Documentation

- [Product specification](docs/PRODUCT_SPEC.md)
- [Technical design](docs/TECHNICAL_DESIGN.md)
- [Implementation plan](docs/IMPLEMENTATION_PLAN.md)
- [macOS parity matrix](docs/MAC_PARITY.md)
- [Windows validation checklist](docs/WINDOWS_VALIDATION.md)
