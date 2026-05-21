using System.Globalization;
using System.Windows.Data;

namespace CMSBuilder.Converters;

public class NullableDoubleToSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d && d > 0)
            return d;
        return double.NaN;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d && !double.IsNaN(d) && d > 0)
            return d;
        return null!;
    }
}
