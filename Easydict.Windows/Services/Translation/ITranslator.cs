namespace Easydict.Windows.Services.Translation;

public interface ITranslator
{
    Task<TranslationResult> TranslateAsync(string text, CancellationToken cancellationToken);
}

public sealed record TranslationResult(string Text);
