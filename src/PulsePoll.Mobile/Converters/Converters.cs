using System.Globalization;
using PulsePoll.Mobile.Helpers;

namespace PulsePoll.Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            var successColor = ThemeHelper.Resolve("Success", "SuccessDark", Colors.Green);
            var dangerColor = ThemeHelper.Resolve("Danger", "DangerDark", Colors.Red);
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
                return ThemeHelper.Resolve("Success", "SuccessDark", Colors.Green);
            return ThemeHelper.Resolve("PrimaryPurple", "PrimaryPurpleDark", Colors.Purple);
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

        return status switch
        {
            "Tamamlandı" or "Completed" => ThemeHelper.Resolve("Success", "SuccessDark", Colors.Green),
            "Devam Ediyor" or "Partial" or "Kısmi" => ThemeHelper.Resolve("Amber", "AmberDark", Colors.Orange),
            "Elendi" or "Diskalifiye" or "Disqualify" or "Elenmiş" or "ScreenOut" =>
                ThemeHelper.Resolve("Danger", "DangerDark", Colors.Red),
            "Kota Dolu" or "QuotaFull" =>
                ThemeHelper.Resolve("PrimaryPurple", "PrimaryPurpleDark", Colors.Purple),
            _ => ThemeHelper.Resolve("TextTertiary", "TextTertiaryDark", Colors.Gray)
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

        return status switch
        {
            "Tamamlandı" or "Completed" => ThemeHelper.Resolve("SuccessLight", "SuccessLightDark", Colors.LightGreen),
            "Devam Ediyor" or "Partial" or "Kısmi" => ThemeHelper.Resolve("AmberLight", "AmberLightDark", Colors.LemonChiffon),
            "Elendi" or "Diskalifiye" or "Disqualify" or "Elenmiş" or "ScreenOut" =>
                ThemeHelper.Resolve("DangerLight", "DangerLightDark", Colors.MistyRose),
            "Kota Dolu" or "QuotaFull" =>
                ThemeHelper.Resolve("PrimaryLight", "PrimaryLightDark", Colors.Lavender),
            _ => ThemeHelper.Resolve("SurfaceColor", "SurfaceColorDark", Colors.LightGray)
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

        return type switch
        {
            "survey" => ThemeHelper.Resolve("PrimaryLight", "PrimaryLightDark", Colors.Lavender),
            "earning" => ThemeHelper.Resolve("AmberLight", "AmberLightDark", Colors.LemonChiffon),
            "rank" => ThemeHelper.Resolve("SuccessLight", "SuccessLightDark", Colors.LightGreen),
            "disqualified" => ThemeHelper.Resolve("DangerLight", "DangerLightDark", Colors.MistyRose),
            _ => ThemeHelper.Resolve("SurfaceColor", "SurfaceColorDark", Colors.LightGray)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class NotificationTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value?.ToString();
        return type switch
        {
            "survey" => "icon_play.png",
            "earning" => "icon_wallet.png",
            "rank" => "icon_star.png",
            "disqualified" => "icon_x_circle.png",
            _ => "icon_bell.png"
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
                return ThemeHelper.Resolve("White", "WhiteDark", Colors.White);
            // Unread → light purple tint
            return ThemeHelper.Resolve("PrimaryLight", "PrimaryLightDark", Color.FromArgb("#EDE8FF"));
        }
        return ThemeHelper.Resolve("White", "WhiteDark", Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

public class StarConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var star = value is int s ? s : 0;
        var filled = new string('★', star);
        var empty = new string('☆', 5 - star);
        return filled + empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class EditButtonTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "İptal" : "Düzenle";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class GenderDisplayConverter : IValueConverter
{
    private static readonly string[] Items = ["Erkek", "Kadın", "Diğer"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i >= 0 && i < Items.Length ? Items[i] : "-";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class MaritalStatusDisplayConverter : IValueConverter
{
    private static readonly string[] Items = ["Bekar", "Evli", "Boşanmış", "Dul"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i >= 0 && i < Items.Length ? Items[i] : "-";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class GsmOperatorDisplayConverter : IValueConverter
{
    private static readonly string[] Items = ["Turkcell", "Vodafone", "Türk Telekom", "Diğer"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i >= 0 && i < Items.Length ? Items[i] : "-";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToYesNoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "Evet" : "Hayır";

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
