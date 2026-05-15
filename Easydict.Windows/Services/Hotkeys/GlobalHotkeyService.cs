using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Easydict.Windows.Services.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly IntPtr windowHandle;
    private readonly HwndSource source;
    private readonly Collection<int> registeredIds = [];
    private bool disposed;

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public GlobalHotkeyService(Window owner)
    {
        windowHandle = new WindowInteropHelper(owner).EnsureHandle();
        source = HwndSource.FromHwnd(windowHandle) ?? throw new InvalidOperationException("Unable to attach to the window message source.");
        source.AddHook(WndProc);
    }

    public void Register(HotkeyAction action, HotkeyModifiers modifiers, int virtualKey)
    {
        Register(action, new HotkeyGesture
        {
            Modifiers = modifiers,
            VirtualKey = virtualKey,
        });
    }

    public void Register(HotkeyAction action, HotkeyGesture gesture)
    {
        var id = (int)action;
        if (!RegisterHotKey(windowHandle, id, (uint)gesture.Modifiers, (uint)gesture.VirtualKey))
        {
            throw new InvalidOperationException($"Unable to register global hotkey for {action}.");
        }

        registeredIds.Add(id);
    }

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WmHotkey)
        {
            return IntPtr.Zero;
        }

        handled = true;
        var action = (HotkeyAction)wParam.ToInt32();
        HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(action));
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        source.RemoveHook(WndProc);
        foreach (var id in registeredIds)
        {
            UnregisterHotKey(windowHandle, id);
        }

        disposed = true;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public enum HotkeyAction
{
    Lookup = 1,
    Ocr = 2,
    SilentOcr = 3,
}

[Flags]
public enum HotkeyModifiers : uint
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
    NoRepeat = 0x4000,
}

public sealed class HotkeyPressedEventArgs(HotkeyAction action) : EventArgs
{
    public HotkeyAction Action { get; } = action;
}

public sealed class HotkeyGesture
{
    public HotkeyModifiers Modifiers { get; set; }
    public int VirtualKey { get; set; }
}
