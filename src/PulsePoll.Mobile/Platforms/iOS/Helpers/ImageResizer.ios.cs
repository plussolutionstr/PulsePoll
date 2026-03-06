using CoreGraphics;
using Foundation;
using UIKit;

namespace PulsePoll.Mobile.Helpers;

public static partial class ImageResizer
{
    public static partial Task<Stream> ResizeAsync(string filePath, int maxWidth, int maxHeight, int quality)
    {
        return Task.Run(() =>
        {
            // UIImage.FromFile can return null for camera-captured temp files; fall back to NSData
            var image = UIImage.FromFile(filePath);
            if (image is null)
            {
                var data = NSData.FromFile(filePath);
                if (data is not null)
                    image = UIImage.LoadFromData(data);
            }

            if (image is null)
                throw new InvalidOperationException($"Fotoğraf yüklenemedi: {filePath}");

            var ratioX = (double)maxWidth / image.Size.Width;
            var ratioY = (double)maxHeight / image.Size.Height;
            var ratio = Math.Min(1.0, Math.Min(ratioX, ratioY));
            var newSize = new CGSize(image.Size.Width * ratio, image.Size.Height * ratio);

            // Use UIGraphicsImageRenderer (iOS 10+) — replaces deprecated UIGraphics.BeginImageContext
            var renderer = new UIGraphicsImageRenderer(newSize);
            var resized = renderer.CreateImage(ctx =>
            {
                image.Draw(new CGRect(CGPoint.Empty, newSize));
            });

            var jpeg = resized.AsJPEG((nfloat)(quality / 100.0));
            if (jpeg is null)
                throw new InvalidOperationException("JPEG dönüşümü başarısız.");

            var ms = new MemoryStream();
            jpeg.AsStream().CopyTo(ms);
            ms.Position = 0;

            image.Dispose();
            resized.Dispose();

            return (Stream)ms;
        });
    }
}
