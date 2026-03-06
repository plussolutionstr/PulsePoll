using CoreGraphics;
using UIKit;

namespace PulsePoll.Mobile.Helpers;

public static partial class ImageResizer
{
    public static partial Task<Stream> ResizeAsync(string filePath, int maxWidth, int maxHeight, int quality)
    {
        return Task.Run(() =>
        {
            // UIImage handles EXIF orientation automatically
            var image = UIImage.FromFile(filePath)!;

            var ratioX = (double)maxWidth / image.Size.Width;
            var ratioY = (double)maxHeight / image.Size.Height;
            var ratio = Math.Min(1.0, Math.Min(ratioX, ratioY));
            var newSize = new CGSize(image.Size.Width * ratio, image.Size.Height * ratio);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1.0f);
            image.Draw(new CGRect(CGPoint.Empty, newSize));
            var resized = UIGraphics.GetImageFromCurrentImageContext()!;
            UIGraphics.EndImageContext();

            var jpeg = resized.AsJPEG((nfloat)(quality / 100.0))!;
            var ms = new MemoryStream();
            jpeg.AsStream().CopyTo(ms);
            ms.Position = 0;

            image.Dispose();
            resized.Dispose();

            return (Stream)ms;
        });
    }
}
