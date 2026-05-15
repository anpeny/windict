namespace Easydict.Windows.Models;

public sealed class QueryHistoryItem
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public string Query { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Source { get; set; } = "manual";
}

