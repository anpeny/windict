using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Easydict.Windows.Services.ScreenCapture;

public sealed class ScreenCaptureService
{
    public BitmapSource CaptureAllScreens()
    {
        var bounds = GetVirtualScreenBounds();
        return CaptureRegion(bounds);
    }

    public BitmapSource CaptureRegion(Rectangle bounds)
    {
        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        }

        var handle = bitmap.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    public static Rectangle GetVirtualScreenBounds()
    {
        var screens = Forms.Screen.AllScreens;
        if (screens.Length == 0)
        {
            return Rectangle.Empty;
        }

        var bounds = screens[0].Bounds;
        foreach (var screen in screens.Skip(1))
        {
            bounds = Rectangle.Union(bounds, screen.Bounds);
        }

        return bounds;
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}
