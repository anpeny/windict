using System.Windows;
using System.Windows.Input;
using Easydict.Windows.Models;
using Easydict.Windows.Services.History;

namespace Easydict.Windows.Views;

public partial class HistoryWindow : Window
{
    private readonly HistoryService historyService;

    public HistoryWindow(HistoryService historyService)
    {
        this.historyService = historyService;
        InitializeComponent();
        RefreshHistory();
    }

    public void RefreshHistory()
    {
        HistoryListBox.ItemsSource = historyService.Load();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        historyService.Clear();
        RefreshHistory();
    }

    private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (HistoryListBox.SelectedItem is QueryHistoryItem item && !string.IsNullOrWhiteSpace(item.Query))
        {
            Clipboard.SetText(item.Query);
        }
    }
}

