using System.Text.RegularExpressions;

namespace frontend.Helpers;

public static class InputFilter
{
    private static readonly Regex _lettersOnly = new(@"[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]", RegexOptions.Compiled);
    private static readonly Regex _cedula = new(@"[^0-9A-Z\-]", RegexOptions.Compiled);
    private static readonly Regex _nonDecimal = new(@"[^0-9.]", RegexOptions.Compiled);
    private static readonly Regex _nonDigit = new(@"[^0-9]", RegexOptions.Compiled);
    private static readonly Regex _alphanumeric = new(@"[^a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\-.,/#]", RegexOptions.Compiled);

    /// <summary>
    /// Allows only letters (with accents) and spaces.
    /// Use for: Name, LastName fields.
    /// </summary>
    public static void AllowLettersOnly(Entry entry, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        var filtered = _lettersOnly.Replace(e.NewTextValue, "");
        if (filtered != e.NewTextValue)
            entry.Text = filtered;
    }

    /// <summary>
    /// Allows digits, dashes, and letters used in Panamanian cedula formats.
    /// </summary>
    public static void AllowCedulaFormat(Entry entry, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        var filtered = _cedula.Replace(e.NewTextValue.ToUpper(), "");
        if (filtered != e.NewTextValue)
            entry.Text = filtered;
    }

    /// <summary>
    /// Allows only digits and a single decimal point.
    /// Use for: Rate, monetary amount fields.
    /// </summary>
    public static void AllowDecimalOnly(Entry entry, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        var filtered = _nonDecimal.Replace(e.NewTextValue, "");
        var dotIndex = filtered.IndexOf('.');
        if (dotIndex >= 0)
        {
            var afterDot = filtered[(dotIndex + 1)..].Replace(".", "");
            filtered = filtered[..(dotIndex + 1)] + afterDot;
        }
        if (filtered != e.NewTextValue)
            entry.Text = filtered;
    }

    /// <summary>
    /// Allows only whole digits (0-9).
    /// Use for: Hours, Minutes fields.
    /// </summary>
    public static void AllowIntegerOnly(Entry entry, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        var filtered = _nonDigit.Replace(e.NewTextValue, "");
        if (filtered != e.NewTextValue)
            entry.Text = filtered;
    }

    /// <summary>
    /// Allows letters, digits, spaces, dashes, and common punctuation.
    /// Use for: Batch name, Location, Activity name fields.
    /// </summary>
    public static void AllowAlphanumeric(Entry entry, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        var filtered = _alphanumeric.Replace(e.NewTextValue, "");
        if (filtered != e.NewTextValue)
            entry.Text = filtered;
    }
}
