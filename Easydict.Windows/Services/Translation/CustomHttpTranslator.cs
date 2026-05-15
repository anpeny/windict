using System.Net.Http;
using System.Text;
using System.Text.Json;
using Easydict.Windows.Models;

namespace Easydict.Windows.Services.Translation;

public sealed class CustomHttpTranslator : ITranslator
{
    private static readonly HttpClient HttpClient = new();
    private readonly TranslationSettings settings;

    public CustomHttpTranslator(TranslationSettings settings)
    {
        this.settings = settings;
    }

    public async Task<TranslationResult> TranslateAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiUrl))
        {
            return new TranslationResult("翻译服务尚未配置。");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.ApiUrl);
        var payload = new Dictionary<string, string>
        {
            [settings.QueryField] = text,
            ["from"] = settings.SourceLanguage,
            ["to"] = settings.TargetLanguage,
        };

        if (settings.RequestBodyType == RequestBodyType.Json)
        {
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        else
        {
            request.Content = new FormUrlEncodedContent(payload);
        }

        if (!string.IsNullOrWhiteSpace(settings.AuthToken))
        {
            request.Headers.TryAddWithoutValidation("Authorization", settings.AuthToken);
        }

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new TranslationResult($"请求失败：{(int)response.StatusCode} {response.ReasonPhrase}\n\n{content}");
        }

        var extracted = ExtractResult(content, settings.ResultPath);
        return new TranslationResult(string.IsNullOrWhiteSpace(extracted) ? content : extracted);
    }

    private static string? ExtractResult(string content, string resultPath)
    {
        if (string.IsNullOrWhiteSpace(resultPath))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var current = document.RootElement;
            foreach (var segment in resultPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var property))
                {
                    current = property;
                    continue;
                }

                if (current.ValueKind == JsonValueKind.Array
                    && int.TryParse(segment, out var index)
                    && index >= 0
                    && index < current.GetArrayLength())
                {
                    current = current[index];
                    continue;
                }

                return null;
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Array => string.Join(Environment.NewLine + Environment.NewLine, current.EnumerateArray().Select(ToDisplayString)),
                JsonValueKind.Object => current.GetRawText(),
                _ => current.ToString(),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ToDisplayString(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var question = TryGetString(element, "question");
            var options = TryGetString(element, "options");
            var answer = TryGetString(element, "answer");
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(question))
            {
                parts.Add($"题目：{question}");
            }

            if (!string.IsNullOrWhiteSpace(options))
            {
                parts.Add($"选项：{options}");
            }

            if (!string.IsNullOrWhiteSpace(answer))
            {
                parts.Add($"答案：{answer}");
            }

            return parts.Count > 0 ? string.Join(Environment.NewLine, parts) : element.GetRawText();
        }

        return element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.GetRawText();
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
    }
}
