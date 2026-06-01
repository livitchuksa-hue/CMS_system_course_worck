using System.Globalization;
using System.Windows.Data;
using CMSBuilder.Helpers;

namespace CMSBuilder.Converters;

public class IntegerStringConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is int i) return i.ToString(culture);
    if (value is string s && int.TryParse(s, out var parsed)) return parsed.ToString(culture);
    if (value is double d) return ((int)Math.Round(d)).ToString(culture);
    return value?.ToString() ?? string.Empty;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    var text = value?.ToString()?.Trim();
    if (string.IsNullOrEmpty(text))
      return targetType == typeof(int?) || Nullable.GetUnderlyingType(targetType) != null ? null : 0;

    if (!PxValueHelper.IsDigitsOnly(text) || !int.TryParse(text, NumberStyles.None, culture, out var n))
      return Binding.DoNothing;

    return n;
  }
}
