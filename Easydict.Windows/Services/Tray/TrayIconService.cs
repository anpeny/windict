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

    public void ShowInfo(string title, string message)
    {
        notifyIcon.BalloonTipTitle = title;
        notifyIcon.BalloonTipText = message;
        notifyIcon.ShowBalloonTip(2500);
    }

    public void Dispose()
    {
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
    }

    private Forms.ContextMenuStrip BuildContextMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示主窗口", null, (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("输入查询", null, (_, _) => InputRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("划词查询", null, (_, _) => LookupRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("全屏 OCR", null, (_, _) => OcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("区域 OCR", null, (_, _) => RegionOcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("静默区域 OCR", null, (_, _) => SilentOcrRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("查询历史", null, (_, _) => HistoryRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("退出 Easydict", null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));
        return menu;
    }
}
