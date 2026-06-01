using System.Globalization;
using System.Windows.Data;
using CMSBuilder.Helpers;

namespace CMSBuilder.Converters;

public class HexColorStringConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    var s = value as string;
    if (string.IsNullOrWhiteSpace(s)) return "#000000";
    return PxValueHelper.NormalizeHexColor(s);
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    var raw = value?.ToString() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(raw)) return "#000000";
    return PxValueHelper.NormalizeHexColor(raw);
  }
}
