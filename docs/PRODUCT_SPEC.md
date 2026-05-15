# Product Specification

## Purpose

Easydict for Windows helps users quickly look up words, translate text, and recognize text from screenshots without disrupting their current workflow.

The app should feel like a lightweight desktop assistant: always available, fast to trigger, quiet when idle, and careful not to steal focus from the app the user is currently using.

## Target Users

- Users who read and write across multiple languages.
- Developers, students, researchers, and office workers who need quick translation.
- Users who frequently need OCR for screenshots, images, documents, or remote desktops.

## Core Workflows

### Selected Text Lookup

1. The user selects text in any Windows app.
2. The user presses the lookup shortcut.
3. Easydict reads the selected text through clipboard or UI Automation fallback.
4. A compact popup appears near the cursor without activating itself.
5. The popup shows dictionary, translation, and optional AI results.

### Manual Text Lookup

1. The user presses the lookup shortcut.
2. Easydict shows a non-activating query popup.
3. The user types or pastes text.
4. Easydict shows translation results.

### Screenshot OCR

1. The user presses the OCR shortcut.
2. Easydict captures the screen or enters a region selection mode.
3. OCR extracts text from the image.
4. The result opens in the query popup and can be translated.

### Silent Screenshot OCR

1. The user presses the silent OCR shortcut.
2. Easydict enters region selection mode.
3. OCR extracts text from the selected image.
4. The recognized text is copied directly to the clipboard.

### Background Operation

1. Easydict starts in the background.
2. A tray icon provides access to settings, enable/disable shortcuts, and quit.
3. Shortcuts continue to work while the main window is hidden.
4. The user can optionally start Easydict when Windows starts.

## Feature Scope

### Version 0.1

- WPF app shell
- Global hotkeys
- Non-activating popup window
- Full-screen screenshot OCR
- Manual lookup popup
- Basic translation service interface

### Version 0.2

- Configurable shortcuts
- Tray icon and background startup mode
- Real translation services
- Clipboard-based selected text lookup
- Region screenshot selection
- Question bank API preset for the provided search endpoint

### Version 0.3

- Service configuration UI
- OCR language selection
- Result history
- Auto-update strategy
- Installer packaging

## Non-Goals for the First Version

- Full parity with every macOS Easydict feature.
- Browser extension integration.
- Mobile or cross-platform UI reuse.
- Complex account sync.

## User Experience Principles

- **Fast:** common actions should appear in under 200 ms when no network request is needed.
- **Quiet:** popup windows should not steal focus or interrupt typing in another app.
- **Predictable:** shortcuts should be configurable and easy to disable.
- **Native:** behaviors should follow Windows desktop expectations.
- **Recoverable:** failed OCR, failed network calls, and shortcut conflicts should produce clear messages.

## Accessibility

- Support keyboard navigation in all windows.
- Maintain readable contrast.
- Avoid tiny click targets.
- Respect system DPI scaling.
- Prepare user-facing strings for future localization.

## Privacy

- Text should only be sent to configured translation providers when the user triggers lookup.
- OCR should run locally by default through Windows OCR.
- API keys and tokens should be stored with Windows Data Protection API or Windows Credential Manager in a later milestone.
