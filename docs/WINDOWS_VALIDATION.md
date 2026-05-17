# Windows Validation Checklist

Use this checklist on a real Windows machine after each desktop-integration change.

## Build

```powershell
.\scripts\build.ps1 -Configuration Debug
.\scripts\build.ps1 -Configuration Release
```

## GitHub Actions

- Push the Windows repository to GitHub.
- Open the `Windows Build` workflow.
- Confirm the `Build WPF app` job passes on `windows-latest`.
- Download the `easydict-windows-win-x64` artifact.
- Run the published executable on a Windows machine.

## Startup

- Launch the app from Visual Studio.
- Launch the app with `dotnet run`.
- Launch a second instance and confirm it exits.
- Close the main window and confirm the tray icon remains.
- Start the app and confirm it can start hidden in the system tray.
- Minimize the main window and confirm it hides to the system tray.
- Confirm the app is not left as a normal taskbar app while hidden.
- Use the tray menu to show the main window.
- Use the tray menu to exit.

## Global Hotkeys

- Press `Ctrl+Alt+D` in Notepad.
- Press `Ctrl+Alt+D` in Edge or Chrome.
- Press `Ctrl+Alt+D` in Visual Studio Code.
- Change the lookup shortcut in settings and save.
- Confirm the old shortcut stops working and the new shortcut works.
- Configure two actions with the same shortcut and confirm the app reports a conflict.
- Confirm the old shortcuts still work after a failed shortcut save.
- Confirm shortcut registration errors are visible.

## Selected Text

- Select text in Notepad and trigger lookup.
- Select text in a browser and trigger lookup.
- Select text in Visual Studio Code and trigger lookup.
- Confirm the clipboard content is restored after lookup.
- Confirm manual popup mode appears when no selected text is available.
- Confirm the manual popup focuses the input field when selected text is unavailable.

## No-Focus Popup

- Type continuously in another app and trigger lookup.
- Confirm the lookup popup appears without stealing focus.
- Confirm the foreground app remains the active app.
- Confirm the popup does not appear in Alt-Tab.

## OCR

- Trigger full-screen OCR.
- Trigger region OCR.
- Press Esc during region selection and confirm it cancels.
- Test on a secondary monitor.
- Test at 100%, 125%, and 150% DPI scaling.
- Confirm OCR language availability on the test machine.

## Translation Provider

- Save a JSON custom provider.
- Save a form custom provider.
- Test Authorization header behavior.
- Test a valid result path.
- Test an invalid result path and confirm raw output appears.
- Test failed HTTP status codes.

## Startup Registration

- Enable launch at login.
- Confirm `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` contains Easydict.
- Disable launch at login.
- Confirm the Run key value is removed.

## Publish

```powershell
.\scripts\publish-win-x64.ps1
```

- Run the published executable.
- Confirm settings persist.
- Confirm tray exit works.
- Confirm logs are written to `%APPDATA%\Easydict\logs` when an exception occurs.
