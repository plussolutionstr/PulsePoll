namespace PulsePoll.Application.Helpers;

public static class PhoneHelper
{
    /// <summary>
    /// Normalizes a Turkish phone number to 10-digit format: 5XXXXXXXXX
    /// Strips spaces, dashes, parentheses, dots.
    /// Removes leading +90, 0090, 90, or 0.
    /// Returns null if the result is not a valid 10-digit Turkish mobile number.
    /// </summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Keep only digits
        var digits = new string(input.Where(char.IsDigit).ToArray());

        // Strip country code prefixes
        if (digits.StartsWith("0090"))
            digits = digits[4..];
        else if (digits.StartsWith("90") && digits.Length > 10)
            digits = digits[2..];

        // Strip leading zero: 05XX → 5XX
        if (digits.StartsWith('0') && digits.Length == 11)
            digits = digits[1..];

        // Must be 10 digits starting with 5
        if (digits.Length != 10 || !digits.StartsWith('5'))
            return null;

        return digits;
    }
}
