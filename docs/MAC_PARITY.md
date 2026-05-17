# macOS Parity Matrix

This document tracks Windows parity against the core macOS Easydict workflows.

## Implemented Core Parity

| macOS Easydict workflow | Windows status | Notes |
| --- | --- | --- |
| Manual input lookup | Implemented | Query popup supports typed text, custom HTTP lookup, and a global input hotkey. |
| Shortcut selected-text lookup | Implemented | Uses UI Automation text selection first, then clipboard preservation. |
| Screenshot OCR lookup | Implemented | Full-screen OCR and region OCR are available. |
| Silent screenshot OCR | Implemented | Region OCR can copy recognized text directly to the clipboard. |
| Non-disruptive popup | Implemented | Uses non-activating WPF window and Win32 extended styles. |
| Background operation | Implemented | Tray icon, hide-on-close, and exit action are available. |
| Configurable shortcuts | Implemented | Lookup, OCR, and silent OCR hotkeys are editable. |
| Query history | Implemented | Local JSON history stores recent queries and results. |
| Custom request service | Implemented | JSON/Form POST, Authorization header, query field, and result path are configurable. |
| Question bank API preset | Implemented | Preset matches the provided `/api/v1/search` endpoint. |
| Build automation | Implemented | GitHub Actions builds Debug, Release, and win-x64 publish artifacts. |

## Partial or Pending Parity

| macOS Easydict capability | Windows status | Reason |
| --- | --- | --- |
| Mouse hover auto-selection lookup | Pending | Requires Windows mouse hooks, UI Automation hit testing, and careful foreground-app behavior. |
| Multiple translation providers | Partial | Current Windows version has a generic custom HTTP provider, not the full macOS provider matrix. |
| System dictionary | Pending | macOS system dictionary has no direct Windows equivalent. A local dictionary provider needs a separate data source. |
| System translation | Pending | macOS system translation has no direct Windows equivalent. |
| TTS | Pending | Can be implemented with `System.Speech` or Windows Speech APIs. |
| 48-language provider matrix | Pending | Requires provider-specific implementations and language mapping. |
| Auto update | Pending | Requires installer and update channel decisions. |

## Validation Required on Windows

The following cannot be proven from macOS:

- WPF compilation.
- Win32 global hotkey registration.
- No-focus popup behavior with real foreground apps.
- Clipboard preservation across common Windows apps.
- Windows Runtime OCR availability and language packs.
- Multi-monitor DPI behavior.
- Tray icon lifecycle.
- Startup registration through the current-user Run key.

Use `docs/WINDOWS_VALIDATION.md` on a Windows machine before considering the app release-ready.
