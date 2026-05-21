using CMSBuilder.Models.Enums;

namespace CMSBuilder.Models;

public class ElementAction
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public int? TargetPageId { get; set; }
    /// <summary>Id элемента-формы для SubmitForm или связанного поля.</summary>
    public int? TargetElementId { get; set; }
    public string? TargetUrl { get; set; }
    public string? CustomJavaScript { get; set; }
    public string? PopupHtml { get; set; }

    public Website Website { get; set; } = null!;
    public Page? TargetPage { get; set; }
    public ICollection<PageElement> Elements { get; set; } = new List<PageElement>();
}
