# Four-Month Optimization Roadmap

This document defines a four-month optimization plan for Easydict for Windows. The goal is to bring the Windows client as close as practical to the macOS Easydict experience while respecting Windows platform differences.

## Guiding Principles

- Keep the app tray-first, lightweight, and non-disruptive.
- Preserve the user's foreground app whenever lookup does not explicitly require input focus.
- Treat global hotkeys, selected-text lookup, OCR, and popup behavior as the core product experience.
- Prefer small, testable service boundaries for Windows-specific integration.
- Keep release artifacts reproducible through GitHub Actions.
- Avoid claiming full macOS parity where Windows lacks equivalent system APIs.

## Month 1: Stability and Real Windows Validation

### Goal

Make the current Windows app reliable on real Windows machines before adding larger feature surfaces.

### Tasks

1. Build a real-device validation matrix.
   - Windows 10 and Windows 11.
   - Single monitor and multiple monitors.
   - 100%, 125%, and 150% DPI scaling.
   - Normal startup, launch-at-login startup, tray startup, and repeated launch.

2. Harden tray lifecycle behavior.
   - Verify start-hidden behavior.
   - Verify minimize-to-tray behavior.
   - Verify close-to-tray behavior.
   - Verify tray exit fully releases hotkeys, tray icon, and background listeners.
   - Verify launching the executable again shows the existing instance.

3. Harden OCR capture.
   - Validate full-screen OCR does not capture a black screen.
   - Validate region OCR coordinates on scaled and multi-monitor displays.
   - Validate empty OCR results clear the popup state.
   - Validate OCR failures restore the previous tray/window state.

4. Harden global hotkeys.
   - Validate default hotkeys work across Notepad, browsers, Visual Studio Code, and Office.
   - Validate conflicts are reported without crashing the app.
   - Validate failed re-registration preserves the previous working hotkeys.
   - Validate startup does not crash when another app already owns a default hotkey.

5. Harden selected-text lookup.
   - Validate UI Automation selection extraction.
   - Validate clipboard fallback in common apps.
   - Preserve text, image, and file-list clipboard contents after fallback copy.

6. Improve diagnostics.
   - Expand logs around startup, hotkey registration, OCR failures, and HTTP provider failures.
   - Add a simple diagnostics export path for logs and settings summary.

### Acceptance Criteria

- The app can run for one week without obvious tray, hotkey, or OCR regressions.
- At least three real Windows machines pass `docs/WINDOWS_VALIDATION.md`.
- No known startup crash remains when hotkeys are occupied.
- OCR failures do not leave the app visible or hidden in the wrong state.

## Month 2: macOS Core Workflow Parity

### Goal

Make the Windows client feel like Easydict in daily workflows, not like a generic WPF settings tool.

### Tasks

1. Refine the lookup popup.
   - Improve placement near cursor and screen-edge avoidance.
   - Keep non-activating display for passive lookup.
   - Use focused input mode only for manual input.
   - Add robust Escape, copy, clear, and retry behavior.

2. Improve selected-text lookup.
   - Tune timing and fallback order for common Windows apps.
   - Add app-specific validation notes for browsers, editors, chat apps, and Office.
   - Avoid stealing focus during passive selected-text lookup.

3. Improve silent OCR.
   - Keep silent OCR tray-first.
   - Copy only recognized text.
   - Show a lightweight tray notification on success or empty result.

4. Improve history.
   - Add search/filter.
   - Add delete-one-item.
   - Add clear confirmation.
   - Add copy query and copy result actions.

5. Reorganize settings.
   - Split settings into sections: General, Hotkeys, OCR, Translation, Tray, Diagnostics.
   - Keep dangerous or advanced settings visually secondary.
   - Validate settings before saving.

6. Refine tray menu.
   - Keep high-frequency actions near the top.
   - Add Settings, History, and Exit.
   - Keep tray labels consistent with the main window.

### Acceptance Criteria

- Users can complete manual lookup, selected-text lookup, full-screen OCR, region OCR, silent OCR, and history lookup without reading documentation.
- Passive lookup paths do not steal focus.
- Settings errors are visible and do not corrupt saved configuration.

## Month 3: Translation, OCR, and Service Expansion

### Goal

Move from a single custom HTTP provider toward a real dictionary and translation platform.

### Tasks

1. Create a provider orchestration layer.
   - Support multiple enabled providers.
   - Support provider ordering.
   - Isolate provider failures.
   - Allow concurrent provider queries with per-provider cancellation.

2. Expand provider support.
   - Keep the custom HTTP provider.
   - Add provider shells for Google, Bing, DeepL, OpenAI, and local dictionary sources.
   - Implement at least three production-usable providers first.

3. Improve question-bank API support.
   - Add a dedicated preset profile.
   - Improve JSON path extraction and array formatting.
   - Add better empty result and error messages.

4. Improve OCR language support.
   - Add OCR language selection in settings.
   - Support `zh-Hans`, `en-US`, and user profile languages.
   - Report unavailable OCR languages clearly.

5. Improve result rendering.
   - Render provider results in separate sections.
   - Add loading, success, empty, and error states per provider.
   - Keep long text readable and copyable.

6. Add text-to-speech.
   - Add basic Windows speech support.
   - Support query text playback and result playback.
   - Add a simple provider capability flag for TTS availability.

### Acceptance Criteria

- At least three translation providers are usable.
- One provider failure does not block other providers.
- OCR language can be configured.
- Result rendering supports multiple providers cleanly.

## Month 4: Productization and Long-Term Maintenance

### Goal

Turn the app into a maintainable, releasable Windows product.

### Tasks

1. Add an installer.
   - Start menu shortcut.
   - Optional desktop shortcut.
   - Uninstall support.
   - Launch-at-login integration.

2. Add update support.
   - Check GitHub Releases for newer versions.
   - Show current version and latest version in settings.
   - Keep updates optional until installer strategy is finalized.

3. Improve diagnostics export.
   - Export logs.
   - Export app version and OS summary.
   - Export sanitized settings summary without secrets.

4. Improve visual polish.
   - Add app icon and tray icon.
   - Improve popup shadow, spacing, and typography.
   - Add dark-mode groundwork.
   - Verify high-DPI layout.

5. Improve privacy and secret storage.
   - Store tokens with Windows DPAPI.
   - Avoid logging secrets.
   - Document clipboard, OCR, and network behavior.

6. Stabilize release workflow.
   - Keep versioned tags.
   - Generate release notes.
   - Keep artifacts reproducible.
   - Add rollback notes for bad releases.

### Acceptance Criteria

- Users can install, run, update-check, and uninstall the app.
- Secrets are not stored as plain text where practical.
- Each release has a repeatable CI path, release notes, and downloadable artifact.

## Highest Priority Backlog

1. Real Windows validation across OS, DPI, and monitor configurations.
2. OCR black-screen, coordinate, and empty-result validation.
3. Hotkey conflict and re-registration reliability.
4. Tray lifecycle and single-instance behavior.
5. Clipboard preservation during selected-text lookup.
6. Non-activating popup behavior in foreground apps.
7. Settings page restructuring.
8. Multi-provider translation architecture.
9. Installer and update-check path.
10. Token encryption with Windows DPAPI.

## Suggested AI Implementation Prompt

Use the following prompt with an AI coding agent that has repository access:

```text
You are working in the Easydict for Windows repository.

Communication with the user must be in Simplified Chinese. All repository documentation and code comments must be written in English.

Goal:
Optimize the Windows version so it is as close as practical to the macOS Easydict experience while keeping native Windows behavior. Follow docs/FOUR_MONTH_OPTIMIZATION_ROADMAP.md, docs/MAC_PARITY.md, docs/WINDOWS_VALIDATION.md, docs/TECHNICAL_DESIGN.md, and AGENTS.md.

Hard requirements:
1. Keep the app tray-first.
2. Preserve foreground focus for passive lookup.
3. Do not break global hotkeys, selected-text lookup, OCR, custom HTTP lookup, history, or GitHub Actions publishing.
4. Do not store or log secrets in plain diagnostic output.
5. Prefer small service-level changes over large rewrites.
6. Keep Windows-specific behavior behind focused service classes.
7. Every completed change must pass GitHub Actions Windows Build.

Work plan:
1. Start with Month 1 stability tasks.
2. Inspect the code before editing.
3. Identify the highest-risk runtime issues that CI cannot catch.
4. Fix issues in small commits.
5. After each fix, run local static checks where possible:
   - git diff --check
   - xmllint for XAML, csproj, manifest, and pubxml files
   - search for unfinished marker strings and placeholder implementation markers
6. Push to GitHub and monitor the Windows Build workflow.
7. If a release artifact is needed, create the next version tag only after main is green.

Immediate priorities:
1. Verify and harden tray lifecycle.
2. Verify and harden hotkey registration and re-registration.
3. Verify and harden OCR capture and state restoration.
4. Verify and harden clipboard preservation.
5. Improve diagnostics enough to debug real Windows reports.

Acceptance:
Produce code changes, documentation updates when needed, passing CI links, release links when tagged, and a concise Chinese summary of what was fixed, what was verified, and what still requires real Windows machine validation.
```
