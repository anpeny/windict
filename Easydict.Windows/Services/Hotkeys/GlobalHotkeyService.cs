using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Easydict.Windows.Services.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private static int nextIdBase;

    private readonly IntPtr windowHandle;
    private readonly HwndSource source;
    private readonly Collection<int> registeredIds = [];
    private readonly Dictionary<int, HotkeyAction> actionsById = [];
    private readonly int idBase;
    private bool disposed;

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public GlobalHotkeyService(Window owner)
    {
        idBase = Interlocked.Add(ref nextIdBase, 16);
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
        var id = idBase + (int)action;
        if (!RegisterHotKey(windowHandle, id, (uint)gesture.Modifiers, (uint)gesture.VirtualKey))
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"Unable to register global hotkey for {action}: {new Win32Exception(error).Message}");
        }

        registeredIds.Add(id);
        actionsById[id] = action;
    }

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WmHotkey)
        {
            return IntPtr.Zero;
        }

        var id = wParam.ToInt32();
        if (!actionsById.TryGetValue(id, out var action))
        {
            return IntPtr.Zero;
        }

        handled = true;
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

        registeredIds.Clear();
        actionsById.Clear();
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
    Input = 4,
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
