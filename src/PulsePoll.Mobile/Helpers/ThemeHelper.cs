namespace PulsePoll.Mobile.Helpers;

public static class ThemeHelper
{
    public static Color Resolve(string lightKey, string darkKey, Color? fallback = null)
    {
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var key = isDark == true ? darkKey : lightKey;
        return Application.Current?.Resources.TryGetValue(key, out var c) == true
            ? (Color)c
            : fallback ?? Colors.Gray;
    }

    public static bool IsDarkTheme =>
        Application.Current?.RequestedTheme == AppTheme.Dark;
}
