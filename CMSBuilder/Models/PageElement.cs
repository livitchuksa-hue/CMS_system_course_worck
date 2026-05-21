using CMSBuilder.Models.Enums;

namespace CMSBuilder.Models;

/// <summary>
/// Универсальная сущность элемента страницы (простые и сложные).
/// Специфичные свойства — в PropertiesJson, стили — в StyleJson.
/// </summary>
public class PageElement
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public int? ParentElementId { get; set; }
    public ElementType Type { get; set; }
    public int SortOrder { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string PropertiesJson { get; set; } = "{}";
    public string StyleJson { get; set; } = "{}";
    public int? ActionId { get; set; }

    public Page Page { get; set; } = null!;
    public PageElement? Parent { get; set; }
    public ICollection<PageElement> Children { get; set; } = new List<PageElement>();
    public ElementAction? Action { get; set; }
}
