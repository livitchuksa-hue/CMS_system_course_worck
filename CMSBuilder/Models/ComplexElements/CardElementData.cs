namespace CMSBuilder.Models.ComplexElements;

/// <summary>Данные комплексного элемента «Карточка» (1:1 с PageElement).</summary>
public class CardElementData
{
    public int PageElementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public PageElement PageElement { get; set; } = null!;
}
