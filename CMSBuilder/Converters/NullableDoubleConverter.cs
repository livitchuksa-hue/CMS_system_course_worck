using System.Globalization;
using System.Windows.Data;

namespace CMSBuilder.Converters;

public class NullableDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return d.ToString(culture);
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return null;
        return double.TryParse(s.Replace(',', '.'), NumberStyles.Any, culture, out var d) ? d : null;
    }
}
