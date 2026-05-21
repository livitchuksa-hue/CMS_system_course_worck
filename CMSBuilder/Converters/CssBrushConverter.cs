using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CMSBuilder.Converters;

public class CssBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return Brushes.Transparent;
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(s)!;
            return new SolidColorBrush(color);
        }
        catch
        {
            return Brushes.Black;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
