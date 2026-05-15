using System.IO;
using System.Windows.Media.Imaging;
using BitmapAlphaMode = global::Windows.Graphics.Imaging.BitmapAlphaMode;
using BitmapDecoder = global::Windows.Graphics.Imaging.BitmapDecoder;
using BitmapPixelFormat = global::Windows.Graphics.Imaging.BitmapPixelFormat;
using DataWriter = global::Windows.Storage.Streams.DataWriter;
using InMemoryRandomAccessStream = global::Windows.Storage.Streams.InMemoryRandomAccessStream;
using OcrEngine = global::Windows.Media.Ocr.OcrEngine;

namespace Easydict.Windows.Services.Ocr;

public sealed class WindowsOcrService : IOcrService
{
    public async Task<string> RecognizeAsync(BitmapSource image, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pngBytes = EncodePng(image);
        using var randomAccessStream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(randomAccessStream))
        {
            writer.WriteBytes(pngBytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
        }

        randomAccessStream.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        var engine = OcrEngine.TryCreateFromUserProfileLanguages();
        if (engine is null)
        {
            return string.Empty;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = await engine.RecognizeAsync(softwareBitmap);
        return result.Text ?? string.Empty;
    }

    private static byte[] EncodePng(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}
