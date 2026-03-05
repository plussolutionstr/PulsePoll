using System.Globalization;

namespace PulsePoll.Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            var successColor = Application.Current!.Resources.TryGetValue("Success", out var s) ? (Color)s : Colors.Green;
            var dangerColor = Application.Current!.Resources.TryGetValue("Danger", out var d) ? (Color)d : Colors.Red;
            return b ? successColor : dangerColor;
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class AmountToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            if (amount > 0)
                return Application.Current!.Resources.TryGetValue("Success", out var s) ? (Color)s : Colors.Green;
            return Application.Current!.Resources.TryGetValue("PrimaryPurple", out var p) ? (Color)p : Colors.Purple;
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.Trim();
        var resources = Application.Current!.Resources;

        return status switch
        {
            "Tamamlandı" or "Completed" => resources.TryGetValue("Success", out var s) ? (Color)s : Colors.Green,
            "Devam Ediyor" or "Partial" or "Kısmi" => resources.TryGetValue("Amber", out var a) ? (Color)a : Colors.Orange,
            "Elendi" or "Diskalifiye" or "Disqualify" or "Elenmiş" or "ScreenOut" =>
                resources.TryGetValue("Danger", out var d) ? (Color)d : Colors.Red,
            "Kota Dolu" or "QuotaFull" =>
                resources.TryGetValue("PrimaryPurple", out var p) ? (Color)p : Colors.Purple,
            _ => resources.TryGetValue("TextTertiary", out var t) ? (Color)t : Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToBgColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.Trim();
        var resources = Application.Current!.Resources;

        return status switch
        {
            "Tamamlandı" or "Completed" => resources.TryGetValue("SuccessLight", out var s) ? (Color)s : Colors.LightGreen,
            "Devam Ediyor" or "Partial" or "Kısmi" => resources.TryGetValue("AmberLight", out var a) ? (Color)a : Colors.LemonChiffon,
            "Elendi" or "Diskalifiye" or "Disqualify" or "Elenmiş" or "ScreenOut" =>
                resources.TryGetValue("DangerLight", out var d) ? (Color)d : Colors.MistyRose,
            "Kota Dolu" or "QuotaFull" =>
                resources.TryGetValue("PrimaryLight", out var p) ? (Color)p : Colors.Lavender,
            _ => resources.TryGetValue("SurfaceColor", out var t) ? (Color)t : Colors.LightGray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.Trim();
        return status switch
        {
            "Tamamlandı" or "Completed" => "✓",
            "Devam Ediyor" or "Partial" or "Kısmi" => "•",
            "Kota Dolu" or "QuotaFull" => "◔",
            "Elendi" or "Diskalifiye" or "Disqualify" or "Elenmiş" or "ScreenOut" => "!",
            _ => "•"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class NotificationTypeToBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value?.ToString();
        var resources = Application.Current!.Resources;

        return type switch
        {
            "survey" => resources.TryGetValue("PrimaryLight", out var p) ? (Color)p : Colors.Lavender,
            "earning" => resources.TryGetValue("AmberLight", out var a) ? (Color)a : Colors.LemonChiffon,
            "rank" => resources.TryGetValue("SuccessLight", out var s) ? (Color)s : Colors.LightGreen,
            "disqualified" => resources.TryGetValue("DangerLight", out var d) ? (Color)d : Colors.MistyRose,
            _ => resources.TryGetValue("SurfaceColor", out var t) ? (Color)t : Colors.LightGray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToReadBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isRead)
        {
            if (isRead)
                return Colors.White;
            // Unread → light purple tint
            return Application.Current!.Resources.TryGetValue("PrimaryLight", out var p) ? (Color)p : Color.FromArgb("#EDE8FF");
        }
        return Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? "icon_check.png" : "icon_x_circle.png";
        return "icon_info.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
