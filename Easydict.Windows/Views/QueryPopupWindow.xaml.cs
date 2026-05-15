using System.Windows;
using Easydict.Windows.Models;
using Easydict.Windows.Services.Interop;
using Easydict.Windows.Services.Translation;
using Forms = System.Windows.Forms;
using WpfClipboard = System.Windows.Clipboard;

namespace Easydict.Windows.Views;

public partial class QueryPopupWindow : Window
{
    private ITranslator translator;
    private CancellationTokenSource? searchCancellation;

    public event EventHandler<QueryHistoryItem>? QueryCompleted;

    public QueryPopupWindow(ITranslator translator)
    {
        this.translator = translator;
        InitializeComponent();
        SourceInitialized += (_, _) => WindowInterop.ApplyNoActivateStyles(this);
    }

    public void SetTranslator(ITranslator nextTranslator)
    {
        translator = nextTranslator;
    }

    public async Task SearchTextAsync(string text)
    {
        QueryTextBox.Text = text;
        await SearchAsync();
    }

    public void ShowNearCursor(string? presetText = null)
    {
        if (!string.IsNullOrWhiteSpace(presetText))
        {
            QueryTextBox.Text = presetText;
        }

        var cursor = Forms.Cursor.Position;
        Left = cursor.X + 14;
        Top = cursor.Y + 14;

        if (!IsVisible)
        {
            Show();
        }

        WindowInterop.ShowNoActivate(this);

        if (!string.IsNullOrWhiteSpace(presetText))
        {
            _ = SearchAsync();
        }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        ResultText.Text = "查询中...";
        searchCancellation?.Cancel();
        searchCancellation = new CancellationTokenSource();

        try
        {
            var result = await translator.TranslateAsync(QueryTextBox.Text, searchCancellation.Token);
            ResultText.Text = result.Text;
            QueryCompleted?.Invoke(this, new QueryHistoryItem
            {
                CreatedAt = DateTimeOffset.Now,
                Query = QueryTextBox.Text.Trim(),
                Result = result.Text.Trim(),
                Source = "lookup",
            });
        }
        catch (OperationCanceledException)
        {
            ResultText.Text = "查询已取消";
        }
        catch (Exception ex)
        {
            ResultText.Text = $"查询失败：{ex.Message}";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void CopyResultButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ResultText.Text))
        {
            WpfClipboard.SetText(ResultText.Text);
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        QueryTextBox.Clear();
        ResultText.Text = "输入内容后点击查询。";
    }
}
