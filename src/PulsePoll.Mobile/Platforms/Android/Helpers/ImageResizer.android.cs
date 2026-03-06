using Android.Graphics;
using Android.Media;
using Stream = System.IO.Stream;

namespace PulsePoll.Mobile.Helpers;

public static partial class ImageResizer
{
    public static partial Task<Stream> ResizeAsync(string filePath, int maxWidth, int maxHeight, int quality)
    {
        return Task.Run(() =>
        {
            // Read EXIF orientation
            var exif = new ExifInterface(filePath);
            var orientation = (Orientation)exif.GetAttributeInt(
                ExifInterface.TagOrientation, (int)Orientation.Normal);

            // Decode bitmap
            var bitmap = BitmapFactory.DecodeFile(filePath)
                         ?? throw new InvalidOperationException($"Fotoğraf yüklenemedi: {filePath}");

            // Apply EXIF rotation
            var matrix = new Matrix();
            switch (orientation)
            {
                case Orientation.Rotate90: matrix.PostRotate(90); break;
                case Orientation.Rotate180: matrix.PostRotate(180); break;
                case Orientation.Rotate270: matrix.PostRotate(270); break;
                case Orientation.FlipHorizontal: matrix.PreScale(-1, 1); break;
                case Orientation.FlipVertical: matrix.PreScale(1, -1); break;
            }

            var rotated = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true)!;
            if (rotated != bitmap) bitmap.Recycle();

            // Scale
            var ratioX = (double)maxWidth / rotated.Width;
            var ratioY = (double)maxHeight / rotated.Height;
            var ratio = Math.Min(1.0, Math.Min(ratioX, ratioY));
            var newW = (int)(rotated.Width * ratio);
            var newH = (int)(rotated.Height * ratio);

            var scaled = Bitmap.CreateScaledBitmap(rotated, newW, newH, true)!;
            if (scaled != rotated) rotated.Recycle();

            // Compress to JPEG
            var ms = new MemoryStream();
            scaled.Compress(Bitmap.CompressFormat.Jpeg!, quality, ms);
            scaled.Recycle();

            ms.Position = 0;
            return (Stream)ms;
        });
    }
}
