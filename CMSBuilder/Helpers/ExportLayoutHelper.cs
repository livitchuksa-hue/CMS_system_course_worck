using System.Text;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Models.Dto;

namespace CMSBuilder.Helpers;

public static class ExportLayoutHelper
{
    public const double CanvasWidth = 960;
    public const double CanvasHeight = 640;

    public static string BuildInlinePositionStyle(PageElement el)
    {
        var props = ElementJsonHelper.DeserializeProperties(el.PropertiesJson);
        var sb = new StringBuilder();
        sb.Append($"left:{el.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        sb.Append($"top:{el.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");

        if (el.Width is > 0)
            sb.Append($"width:{el.Width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        else if (!string.IsNullOrEmpty(props.Text) && el.Type == Models.Enums.ElementType.Image)
            sb.Append("width:auto;");

        if (el.Height is > 0)
            sb.Append($"height:{el.Height.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");

        if (props.IsScalable)
            sb.Append("max-width:100%;");

        return sb.ToString();
    }

    public static string BuildElementCssRules(PageElement el)
    {
        var style = ElementJsonHelper.DeserializeStyle(el.StyleJson);
        var props = ElementJsonHelper.DeserializeProperties(el.PropertiesJson);
        var parts = new List<string>
        {
            "position:absolute",
            $"left:{el.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}px",
            $"top:{el.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}px"
        };

        if (el.Width is > 0)
            parts.Add($"width:{el.Width}px");
        if (el.Height is > 0)
            parts.Add($"height:{el.Height}px");
        if (props.IsScalable)
            parts.Add("max-width:100%");

        if (!string.IsNullOrEmpty(style.FontSize)) parts.Add($"font-size:{style.FontSize}");
        if (!string.IsNullOrEmpty(style.FontFamily)) parts.Add($"font-family:{style.FontFamily}");
        if (!string.IsNullOrEmpty(style.Color)) parts.Add($"color:{style.Color}");
        if (!string.IsNullOrEmpty(style.BackgroundColor)) parts.Add($"background-color:{style.BackgroundColor}");
        if (!string.IsNullOrEmpty(style.TextAlign)) parts.Add($"text-align:{style.TextAlign}");
        if (!string.IsNullOrEmpty(style.Margin)) parts.Add($"margin:{style.Margin}");
        if (!string.IsNullOrEmpty(style.Padding)) parts.Add($"padding:{style.Padding}");
        if (!string.IsNullOrEmpty(style.BorderRadius)) parts.Add($"border-radius:{style.BorderRadius}");
        if (!string.IsNullOrEmpty(style.FontWeight)) parts.Add($"font-weight:{style.FontWeight}");
        if (!string.IsNullOrEmpty(style.BorderColor)) parts.Add($"border-color:{style.BorderColor}");
        if (!string.IsNullOrEmpty(style.BorderWidth)) parts.Add($"border-width:{style.BorderWidth}");
        if (!string.IsNullOrEmpty(style.BorderColor) || !string.IsNullOrEmpty(style.BorderWidth))
            parts.Add("border-style:solid");

        return string.Join(";", parts);
    }

    public static string BuildRelativePositionStyle(PageElement el)
    {
        var style = ElementJsonHelper.DeserializeStyle(el.StyleJson);
        var sb = new StringBuilder("position:relative;");
        sb.Append($"left:{el.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        sb.Append($"top:{el.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        if (el.Width is > 0)
            sb.Append($"width:{el.Width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        if (el.Height is > 0)
            sb.Append($"height:{el.Height.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;");
        if (!string.IsNullOrEmpty(style.BackgroundColor))
            sb.Append($"background-color:{style.BackgroundColor};");
        return sb.ToString();
    }
}
