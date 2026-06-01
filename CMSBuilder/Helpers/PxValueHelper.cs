using System.Globalization;
using System.Text.RegularExpressions;

namespace CMSBuilder.Helpers;

public static class PxValueHelper
{
  private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);
  private static readonly Regex HexColorRegex = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

  public static bool IsDigitsOnly(string? value) =>
      !string.IsNullOrEmpty(value) && DigitsOnly.IsMatch(value.Trim());

  public static string StripPx(string? value)
  {
    if (string.IsNullOrWhiteSpace(value)) return string.Empty;
    var s = value.Trim();
    if (s.EndsWith("px", StringComparison.OrdinalIgnoreCase))
      s = s[..^2].Trim();
    return s;
  }

  public static string AppendPx(string? digits)
  {
    if (string.IsNullOrWhiteSpace(digits)) return string.Empty;
    var d = digits.Trim();
    return IsDigitsOnly(d) ? d + "px" : string.Empty;
  }

  public static string NormalizeHexColor(string? input)
  {
    if (string.IsNullOrWhiteSpace(input)) return "#000000";

    var raw = input.Trim();
    if (!raw.StartsWith('#'))
      raw = "#" + raw;

    var hex = new string(raw.Skip(1).Where(Uri.IsHexDigit).Take(6).ToArray());
    hex = hex.PadRight(6, '0');
    return "#" + hex.ToUpperInvariant();
  }

  public static bool IsValidHexColor(string? value) =>
      !string.IsNullOrWhiteSpace(value) && HexColorRegex.IsMatch(NormalizeHexColor(value));

  public static bool TryParsePositiveDouble(string? text, out double result)
  {
    result = 0;
    if (string.IsNullOrWhiteSpace(text) || !IsDigitsOnly(text)) return false;
    return double.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out result) && result >= 0;
  }
}
