using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CMSBuilder.Converters;

public class CssBorderThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return new Thickness(1);
        s = s.Trim().ToLowerInvariant();
        if (s.EndsWith("px") && double.TryParse(s[..^2], NumberStyles.Any, culture, out var px))
            return new Thickness(px);
        return double.TryParse(s, NumberStyles.Any, culture, out var n) ? new Thickness(n) : new Thickness(1);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
