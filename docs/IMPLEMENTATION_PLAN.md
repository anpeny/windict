# Implementation Plan

## Milestone 1: Native Desktop Skeleton

Status: started.

Goals:

- Create a standalone Windows project outside the macOS repository.
- Build a WPF app shell.
- Register global shortcuts.
- Show a non-activating popup near the cursor.
- Add screenshot capture and OCR service abstractions.
- Add a translation service abstraction.

Exit criteria:

- The app builds on Windows.
- `Ctrl+Alt+D` opens the lookup popup.
- `Ctrl+Alt+S` captures the screen and attempts OCR.
- The popup appears without stealing focus from the current foreground app.

Current progress:

- WPF app shell is in place.
- Global hotkeys are registered.
- The lookup popup uses non-activating window behavior.
- Full-screen capture and Windows OCR plumbing are in place.
- Region screenshot selection is in place.
- Tray background operation is in place.
- Lookup can attempt to read selected text through clipboard preservation.
- Settings are persisted under `%APPDATA%\Easydict\settings.json`.
- Custom HTTP translation provider is in place.
- Shortcut strings can be edited from the main window.
- UI Automation selected-text fallback is in place.
- Windows publish profile and PowerShell build scripts are in place.
- Current-user launch-at-login registration is in place.
- Single-instance and crash logging are in place.
- Query history is persisted and accessible from the main window and tray.
- The tray menu can trigger lookup, full-screen OCR, region OCR, and history.
- GitHub Actions builds Debug, Release, and a self-contained win-x64 artifact.
- Silent region OCR copies recognized text directly to the clipboard.

## Milestone 2: Reliable Lookup Workflow

Goals:

- Implement selected text capture.
- Preserve and restore clipboard content.
- Add lookup workflow orchestration.
- Add proper error messages for empty selection, shortcut conflicts, and OCR failures.
- Add cancellation for in-flight lookup requests.

Exit criteria:

- Selected text lookup works in common apps such as Edge, Chrome, Notepad, Word, and Visual Studio Code.
- Clipboard content is restored after lookup.
- Failed selected text capture falls back to manual popup input.

## Milestone 3: Translation Providers

Goals:

- Add provider interfaces that can model dictionary, translation, OCR, and AI services.
- Implement at least one simple HTTP translation provider.
- Add provider configuration models.
- Add secure token storage.

Exit criteria:

- The user can configure one provider.
- Lookup results appear in the popup.
- Provider errors are visible and recoverable.

## Milestone 4: OCR Region Selection

Goals:

- Add a transparent region selection overlay.
- Capture only the selected region.
- Run OCR on the selected image.
- Send OCR text into the lookup workflow.

Exit criteria:

- The user can select a screen region.
- OCR text appears in the popup.
- The selection overlay behaves correctly across DPI-scaled monitors.

## Milestone 5: Tray and Settings

Goals:

- Add tray icon.
- Add settings window.
- Add configurable shortcuts.
- Add startup behavior.
- Add provider management.

Exit criteria:

- The app can run primarily from the tray.
- Shortcuts can be changed without editing files.
- Settings persist across restarts.

## Milestone 6: Packaging

Goals:

- Add publish profile.
- Add MSIX or installer pipeline.
- Add icon and app metadata.
- Add update strategy.

Exit criteria:

- A tester can install and run Easydict for Windows on a clean Windows machine.
- The app can be uninstalled cleanly.

## Immediate Next Tasks

1. Build the current project on Windows and fix compile issues.
2. Validate non-activating popup behavior with real foreground apps.
3. Validate clipboard preservation in common apps.
4. Test Windows Runtime OCR language availability.
5. Replace the temporary settings surface with a dedicated settings window.
