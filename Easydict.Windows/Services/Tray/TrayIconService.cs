using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Easydict.Windows.Services.Tray;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon notifyIcon;

    public event EventHandler? ShowRequested;
    public event EventHandler? InputRequested;
    public event EventHandler? LookupRequested;
    public event EventHandler? OcrRequested;
    public event EventHandler? RegionOcrRequested;
    public event EventHandler? SilentOcrRequested;
    public event EventHandler? HistoryRequested;
    public event EventHandler? ExitRequested;

    public TrayIconService()
    {
        notifyIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Application,
            Text = "Easydict",
            ContextMenuStrip = BuildContextMenu(),
        };

        notifyIcon.DoubleClick += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Show()
    {
        notifyIcon.Visible = true;
    }

    public void Dispose()
    {
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
    }

    private Forms.ContextMenuStrip BuildContextMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Show", null, (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Input Lookup", null, (_, _) => InputRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Lookup", null, (_, _) => LookupRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Full Screen OCR", null, (_, _) => OcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Region OCR", null, (_, _) => RegionOcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Silent Region OCR", null, (_, _) => SilentOcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("History", null, (_, _) => HistoryRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));
        return menu;
    }
}
