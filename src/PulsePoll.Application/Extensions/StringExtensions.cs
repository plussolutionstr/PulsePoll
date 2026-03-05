using System.Globalization;

namespace PulsePoll.Application.Extensions;

public static class StringExtensions
{
    private static readonly TextInfo TrTextInfo = new CultureInfo("tr-TR", false).TextInfo;

    public static string ToTitleCaseTr(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return TrTextInfo.ToTitleCase(value.Trim().ToLower(new CultureInfo("tr-TR")));
    }
}
