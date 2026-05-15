namespace Easydict.Windows.Services.Translation;

public sealed class EchoTranslator : ITranslator
{
    public Task<TranslationResult> TranslateAsync(string text, CancellationToken cancellationToken)
    {
        var trimmedText = text.Trim();
        if (trimmedText.Length == 0)
        {
            return Task.FromResult(new TranslationResult("请输入要查询的内容。"));
        }

        var result = $"翻译服务尚未配置。\n\n{trimmedText}";
        return Task.FromResult(new TranslationResult(result));
    }
}

