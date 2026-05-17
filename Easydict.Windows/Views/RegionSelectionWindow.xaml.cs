using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Easydict.Windows.Services.ScreenCapture;
using WpfKey = System.Windows.Input.Key;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;

namespace Easydict.Windows.Views;

public partial class RegionSelectionWindow : Window
{
    private WpfPoint? startPoint;
    private Rectangle? selectedRegion;

    public RegionSelectionWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => ApplyVirtualScreenBounds();
        KeyDown += RegionSelectionWindow_KeyDown;
    }

    private void ApplyVirtualScreenBounds()
    {
        var bounds = ScreenCaptureService.GetVirtualScreenBounds();
        var topLeft = PointFromScreen(new WpfPoint(bounds.Left, bounds.Top));
        var bottomRight = PointFromScreen(new WpfPoint(bounds.Right, bounds.Bottom));
        Left = topLeft.X;
        Top = topLeft.Y;
        Width = Math.Abs(bottomRight.X - topLeft.X);
        Height = Math.Abs(bottomRight.Y - topLeft.Y);
    }

    public Rectangle? SelectRegion()
    {
        ShowDialog();
        return selectedRegion;
    }

    private void SelectionCanvas_MouseDown(object sender, WpfMouseButtonEventArgs e)
    {
        startPoint = e.GetPosition(this);
        SelectionRectangle.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRectangle, startPoint.Value.X);
        Canvas.SetTop(SelectionRectangle, startPoint.Value.Y);
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        CaptureMouse();
    }

    private void SelectionCanvas_MouseMove(object sender, WpfMouseEventArgs e)
    {
        if (startPoint is null)
        {
            return;
        }

        var current = e.GetPosition(this);
        var left = Math.Min(startPoint.Value.X, current.X);
        var top = Math.Min(startPoint.Value.Y, current.Y);
        var width = Math.Abs(current.X - startPoint.Value.X);
        var height = Math.Abs(current.Y - startPoint.Value.Y);

        Canvas.SetLeft(SelectionRectangle, left);
        Canvas.SetTop(SelectionRectangle, top);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private void SelectionCanvas_MouseUp(object sender, WpfMouseButtonEventArgs e)
    {
        if (startPoint is null)
        {
            return;
        }

        var current = e.GetPosition(this);
        var screenStart = PointToScreen(startPoint.Value);
        var screenCurrent = PointToScreen(current);
        var screenLeft = Math.Min(screenStart.X, screenCurrent.X);
        var screenTop = Math.Min(screenStart.Y, screenCurrent.Y);
        var screenWidth = Math.Abs(screenCurrent.X - screenStart.X);
        var screenHeight = Math.Abs(screenCurrent.Y - screenStart.Y);

        selectedRegion = new Rectangle(
            (int)Math.Round(screenLeft),
            (int)Math.Round(screenTop),
            (int)Math.Round(screenWidth),
            (int)Math.Round(screenHeight));

        ReleaseMouseCapture();
        Close();
    }

    private void RegionSelectionWindow_KeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key == WpfKey.Escape)
        {
            selectedRegion = null;
            Close();
        }
    }
}
