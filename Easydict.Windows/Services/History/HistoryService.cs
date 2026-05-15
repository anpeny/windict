using System.Text.Json;
using Easydict.Windows.Models;

namespace Easydict.Windows.Services.History;

public sealed class HistoryService
{
    private const int MaxHistoryCount = 200;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public IReadOnlyList<QueryHistoryItem> Load()
    {
        if (!File.Exists(HistoryPath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(HistoryPath);
            return JsonSerializer.Deserialize<List<QueryHistoryItem>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Add(QueryHistoryItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Query))
        {
            return;
        }

        var items = Load()
            .Prepend(item)
            .Take(MaxHistoryCount)
            .ToList();
        Save(items);
    }

    public void Clear()
    {
        Save([]);
    }

    private static void Save(IReadOnlyList<QueryHistoryItem> items)
    {
        Directory.CreateDirectory(HistoryDirectory);
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(HistoryPath, json);
    }

    private static string HistoryDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Easydict");

    private static string HistoryPath => Path.Combine(HistoryDirectory, "history.json");
}

