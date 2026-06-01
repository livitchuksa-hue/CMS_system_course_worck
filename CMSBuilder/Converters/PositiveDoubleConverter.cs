using System.Globalization;
using System.Windows.Data;
using CMSBuilder.Helpers;

namespace CMSBuilder.Converters;

/// <summary>
/// Для ширины/высоты страницы и координат: ввод только цифр.
/// </summary>
public class PositiveDoubleConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is double d && d > 0)
      return ((int)Math.Round(d)).ToString(culture);
    if (value is int i && i > 0)
      return i.ToString(culture);
    return string.Empty;
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    var text = value?.ToString()?.Trim();
    if (string.IsNullOrEmpty(text)) return 0d;
    if (!PxValueHelper.TryParsePositiveDouble(text, out var d)) return Binding.DoNothing;
    return d;
  }
}
