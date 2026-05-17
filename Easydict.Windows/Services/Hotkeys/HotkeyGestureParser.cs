using System.Windows.Input;

namespace Easydict.Windows.Services.Hotkeys;

public static class HotkeyGestureParser
{
    public static HotkeyGesture Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("Hotkey cannot be empty.");
        }

        var parts = text
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        if (parts.Length == 0)
        {
            throw new FormatException("Hotkey cannot be empty.");
        }

        var modifiers = HotkeyModifiers.NoRepeat;
        Key? key = null;

        foreach (var part in parts)
        {
            switch (part.ToUpperInvariant())
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= HotkeyModifiers.Control;
                    break;
                case "ALT":
                    modifiers |= HotkeyModifiers.Alt;
                    break;
                case "SHIFT":
                    modifiers |= HotkeyModifiers.Shift;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= HotkeyModifiers.Windows;
                    break;
                default:
                    key = ParseKey(part);
                    break;
            }
        }

        if (key is null)
        {
            throw new FormatException("Hotkey must include a key.");
        }

        return new HotkeyGesture
        {
            Modifiers = modifiers,
            VirtualKey = KeyInterop.VirtualKeyFromKey(key.Value),
        };
    }

    public static string ToDisplayText(HotkeyGesture gesture)
    {
        var parts = new List<string>();
        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (gesture.Modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        var key = KeyInterop.KeyFromVirtualKey(gesture.VirtualKey);
        parts.Add(new KeyConverter().ConvertToString(key) ?? key.ToString());
        return string.Join("+", parts);
    }

    public static string ToInvariantText(HotkeyGesture gesture)
    {
        return $"{(uint)gesture.Modifiers}:{gesture.VirtualKey}";
    }

    private static Key ParseKey(string text)
    {
        var converter = new KeyConverter();
        var converted = converter.ConvertFromString(text);
        if (converted is Key key && key != Key.None)
        {
            return key;
        }

        throw new FormatException($"Unsupported hotkey key: {text}");
    }
}
