using System.Globalization;
using System.Windows.Data;
using CMSBuilder.Helpers;

namespace CMSBuilder.Converters;

/// <summary>
/// В UI — только цифры; в модели/БД — значение с суффиксом px.
/// </summary>
public class PxStringConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      PxValueHelper.StripPx(value as string);

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    var digits = value?.ToString()?.Trim();
    if (string.IsNullOrEmpty(digits)) return string.Empty;
    if (!PxValueHelper.IsDigitsOnly(digits)) return Binding.DoNothing;
    return PxValueHelper.AppendPx(digits);
  }
}
