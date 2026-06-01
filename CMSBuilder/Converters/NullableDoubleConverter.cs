using System.Globalization;
using System.Windows.Data;
using CMSBuilder.Helpers;

namespace CMSBuilder.Converters;

public class NullableDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return ((int)Math.Round(d)).ToString(culture);
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!PxValueHelper.TryParsePositiveDouble(s, out var d)) return Binding.DoNothing;
        return d;
    }
}
