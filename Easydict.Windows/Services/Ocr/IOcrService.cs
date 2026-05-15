using System.Windows.Media.Imaging;

namespace Easydict.Windows.Services.Ocr;

public interface IOcrService
{
    Task<string> RecognizeAsync(BitmapSource image, CancellationToken cancellationToken);
}

