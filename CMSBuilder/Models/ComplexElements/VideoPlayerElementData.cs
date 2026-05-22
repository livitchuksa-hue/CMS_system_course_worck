namespace CMSBuilder.Models.ComplexElements;

/// <summary>Данные комплексного HTML5-видеоплеера (1:1 с PageElement).</summary>
public class VideoPlayerElementData
{
    public int PageElementId { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public bool ShowControls { get; set; } = true;
    public bool Autoplay { get; set; }
    public bool Loop { get; set; }

    public PageElement PageElement { get; set; } = null!;
}
