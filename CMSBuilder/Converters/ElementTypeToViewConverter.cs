using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CMSBuilder.Models.Enums;
using CMSBuilder.Views.Elements;

namespace CMSBuilder.Converters;

public class ElementTypeToViewConverter : System.Windows.Data.IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ElementType type) return null;
        UserControl? view = type switch
        {
            ElementType.Text => new TextElementView(),
            ElementType.Header => new HeaderElementView(),
            ElementType.Image => new ImageElementView(),
            ElementType.Button => new ButtonElementView(),
            ElementType.Input => new InputElementView(),
            ElementType.Container => new ContainerElementView(),
            ElementType.Navbar => new NavbarElementView(),
            ElementType.Gallery => new GalleryElementView(),
            ElementType.CommentsBlock => new CommentsBlockElementView(),
            ElementType.Form => new FormElementView(),
            ElementType.Card => new CardElementView(),
            ElementType.Checkbox => new CheckboxElementView(),
            ElementType.Link => new LinkElementView(),
            ElementType.TextArea => new TextAreaElementView(),
            ElementType.Footer => new FooterElementView(),
            _ => new TextElementView()
        };
        return view;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
