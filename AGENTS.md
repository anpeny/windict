# AGENTS.md

Use Simplified Chinese for all communication with the user. All documentation and code comments in this repository must be written in English.

## Project Overview

Easydict for Windows is a native Windows dictionary, translation, and OCR lookup app inspired by the macOS Easydict experience.

The Windows client is intentionally maintained outside the macOS repository so each platform can use the most natural native stack and release process.

## Technology Stack

- C# and .NET 8
- WPF for the desktop UI
- Win32 interop for global hotkeys, non-activating windows, screen capture, and low-level desktop behavior
- Windows Runtime OCR (`Windows.Media.Ocr`) as the first built-in OCR provider

## Architecture Rules

- Keep platform integration behind small service interfaces.
- Prefer WPF and Win32 APIs that are stable on Windows 10 2004 and newer.
- Keep UI code thin; put native integration, OCR, and translation behavior in services.
- Do not block the UI thread with network, OCR, or capture work.
- Non-activating lookup windows must preserve the user's current foreground app whenever possible.
- Treat global hotkeys as user-configurable in future settings work.

## Code Style

- Use nullable reference types.
- Prefer async APIs for OCR, translation, and network operations.
- Keep comments concise and only use them where the code is not self-explanatory.
- Keep user-facing strings ready for future localization, even before a full localization system exists.

## Build

Build on Windows with:

```powershell
dotnet build .\Easydict.Windows\Easydict.Windows.csproj
```

The project cannot be fully built on macOS because it targets WPF and Windows-specific APIs.

