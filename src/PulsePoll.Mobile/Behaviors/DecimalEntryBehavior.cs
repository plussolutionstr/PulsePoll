using System.Globalization;
using Microsoft.Maui.Controls;

namespace PulsePoll.Mobile.Behaviors;

public class DecimalEntryBehavior : Behavior<Entry>
{
    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    private Entry? _entry;
    private bool _isUpdating;

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        _entry = entry;
        entry.Keyboard = Keyboard.Numeric;
        entry.TextChanged += OnTextChanged;
        entry.Unfocused += OnUnfocused;
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        entry.Unfocused -= OnUnfocused;
        _entry = null;
        base.OnDetachingFrom(entry);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _entry is null)
            return;

        var input = e.NewTextValue ?? string.Empty;
        var cleaned = CleanInput(input);
        if (cleaned == input)
            return;

        _isUpdating = true;
        _entry.Text = cleaned;
        _isUpdating = false;
    }

    private void OnUnfocused(object? sender, FocusEventArgs e)
    {
        if (_isUpdating || _entry is null)
            return;

        var text = _entry.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        var normalized = text.Replace(".", string.Empty).Replace(",", ".");
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return;

        _isUpdating = true;
        _entry.Text = value.ToString("N2", TrCulture);
        _isUpdating = false;
    }

    private static string CleanInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var chars = new List<char>(input.Length);
        var hasSeparator = false;
        var fractionalCount = 0;

        foreach (var c in input)
        {
            if (char.IsDigit(c))
            {
                if (hasSeparator)
                {
                    if (fractionalCount >= 2)
                        continue;

                    fractionalCount++;
                }

                chars.Add(c);
                continue;
            }

            if ((c == ',' || c == '.') && !hasSeparator)
            {
                chars.Add(',');
                hasSeparator = true;
            }
        }

        return new string(chars.ToArray());
    }
}
