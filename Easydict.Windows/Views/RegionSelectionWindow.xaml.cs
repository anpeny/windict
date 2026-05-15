using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Easydict.Windows.Services.ScreenCapture;
using WpfPoint = System.Windows.Point;

namespace Easydict.Windows.Views;

public partial class RegionSelectionWindow : Window
{
    private WpfPoint? startPoint;
    private Rectangle? selectedRegion;

    public RegionSelectionWindow()
    {
        InitializeComponent();
        var bounds = ScreenCaptureService.GetVirtualScreenBounds();
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        KeyDown += RegionSelectionWindow_KeyDown;
    }

    public Rectangle? SelectRegion()
    {
        ShowDialog();
        return selectedRegion;
    }

    private void SelectionCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        startPoint = e.GetPosition(this);
        SelectionRectangle.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRectangle, startPoint.Value.X);
        Canvas.SetTop(SelectionRectangle, startPoint.Value.Y);
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
        CaptureMouse();
    }

    private void SelectionCanvas_MouseMove(object sender, MouseEventArgs e)
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

    private void SelectionCanvas_MouseUp(object sender, MouseButtonEventArgs e)
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

        selectedRegion = new Rectangle(
            (int)Math.Round(Left + left),
            (int)Math.Round(Top + top),
            (int)Math.Round(width),
            (int)Math.Round(height));

        ReleaseMouseCapture();
        Close();
    }

    private void RegionSelectionWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            selectedRegion = null;
            Close();
        }
    }
}
