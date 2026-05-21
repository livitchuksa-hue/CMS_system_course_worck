namespace CMSBuilder.Models.Dto;

public class ElementStyleDto : NotifyDto
{
    private string? _fontSize;
    private string? _color;
    private string? _backgroundColor;
    private string? _textAlign;
    private string? _margin;
    private string? _padding;
    private string? _borderRadius;
    private string? _width;
    private string? _height;
    private string? _fontWeight;
    private string? _fontFamily;
    private string? _borderColor;
    private string? _borderWidth;

    public string? FontSize { get => _fontSize; set => SetField(ref _fontSize, value); }
    public string? Color { get => _color; set => SetField(ref _color, value); }
    public string? BackgroundColor { get => _backgroundColor; set => SetField(ref _backgroundColor, value); }
    public string? TextAlign { get => _textAlign; set => SetField(ref _textAlign, value); }
    public string? Margin { get => _margin; set => SetField(ref _margin, value); }
    public string? Padding { get => _padding; set => SetField(ref _padding, value); }
    public string? BorderRadius { get => _borderRadius; set => SetField(ref _borderRadius, value); }
    public string? Width { get => _width; set => SetField(ref _width, value); }
    public string? Height { get => _height; set => SetField(ref _height, value); }
    public string? FontWeight { get => _fontWeight; set => SetField(ref _fontWeight, value); }
    public string? FontFamily { get => _fontFamily; set => SetField(ref _fontFamily, value); }
    public string? BorderColor { get => _borderColor; set => SetField(ref _borderColor, value); }
    public string? BorderWidth { get => _borderWidth; set => SetField(ref _borderWidth, value); }
}
