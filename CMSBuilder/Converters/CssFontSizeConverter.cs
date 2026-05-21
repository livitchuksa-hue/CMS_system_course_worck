using System.Globalization;
using System.Windows.Data;

namespace CMSBuilder.Converters;

public class CssFontSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return 14.0;
        s = s.Replace("px", "", StringComparison.OrdinalIgnoreCase).Trim();
        return double.TryParse(s, NumberStyles.Any, culture, out var d) ? d : 14.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() ?? "14";
}
