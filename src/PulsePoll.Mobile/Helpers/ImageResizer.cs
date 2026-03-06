namespace PulsePoll.Mobile.Helpers;

public static partial class ImageResizer
{
    public static partial Task<Stream> ResizeAsync(string filePath, int maxWidth, int maxHeight, int quality);
}
