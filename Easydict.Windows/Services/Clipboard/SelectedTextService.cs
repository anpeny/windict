using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using WpfClipboard = System.Windows.Clipboard;
using WpfDataObject = System.Windows.IDataObject;

namespace Easydict.Windows.Services.Clipboard;

public sealed class SelectedTextService
{
    public async Task<string?> TryReadSelectedTextAsync(TimeSpan copyDelay, CancellationToken cancellationToken)
    {
        var automationText = TryReadSelectedTextWithAutomation();
        if (!string.IsNullOrWhiteSpace(automationText))
        {
            return automationText;
        }

        return await TryReadSelectedTextWithClipboardAsync(copyDelay, cancellationToken);
    }

    private static string? TryReadSelectedTextWithAutomation()
    {
        try
        {
            var focusedElement = AutomationElement.FocusedElement;
            if (focusedElement is null)
            {
                return null;
            }

            if (focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObject)
                && textPatternObject is TextPattern textPattern)
            {
                var text = string.Join(
                    Environment.NewLine,
                    textPattern.GetSelection()
                        .Select(range => range.GetText(MaxAutomationTextLength))
                        .Where(value => !string.IsNullOrWhiteSpace(value)));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }

        }
        catch (ElementNotAvailableException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (COMException)
        {
            return null;
        }

        return null;
    }

    private static async Task<string?> TryReadSelectedTextWithClipboardAsync(TimeSpan copyDelay, CancellationToken cancellationToken)
    {
        WpfDataObject? previousClipboard = null;
        try
        {
            previousClipboard = WpfClipboard.GetDataObject();
        }
        catch (ExternalException)
        {
            previousClipboard = null;
        }

        string? copiedText = null;
        var sequenceBeforeCopy = GetClipboardSequenceNumber();
        try
        {
            SendCopyShortcut();
            await Task.Delay(copyDelay, cancellationToken);

            if (GetClipboardSequenceNumber() != sequenceBeforeCopy)
            {
                try
                {
                    copiedText = WpfClipboard.ContainsText() ? WpfClipboard.GetText() : null;
                }
                catch (ExternalException)
                {
                    copiedText = null;
                }
            }
        }
        finally
        {
            if (previousClipboard is not null)
            {
                try
                {
                    WpfClipboard.SetDataObject(previousClipboard, true);
                }
                catch (ExternalException)
                {
                    // Clipboard ownership can fail when another app changes it at the same time.
                }
            }
        }

        return string.IsNullOrWhiteSpace(copiedText) ? null : copiedText;
    }

    private static void SendCopyShortcut()
    {
        var inputs = new[]
        {
            KeyboardInput(VirtualKeyControl, 0),
            KeyboardInput((ushort)'C', 0),
            KeyboardInput((ushort)'C', KeyEventKeyUp),
            KeyboardInput(VirtualKeyControl, KeyEventKeyUp),
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
    }

    private static Input KeyboardInput(ushort virtualKey, uint flags)
    {
        return new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInputData
                {
                    VirtualKey = virtualKey,
                    Scan = 0,
                    Flags = flags,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero,
                },
            },
        };
    }

    private const int InputKeyboard = 1;
    private const int MaxAutomationTextLength = 16_384;
    private const ushort VirtualKeyControl = 0x11;
    private const uint KeyEventKeyUp = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeyboardInputData Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInputData
    {
        public ushort VirtualKey;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
}
