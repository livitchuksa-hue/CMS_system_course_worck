namespace CMSBuilder.Models.ComplexElements;

/// <summary>Настройки блока просмотра комментариев (1:1 с PageElement).</summary>
public class CommentsViewerElementData
{
    public int PageElementId { get; set; }
    public string Title { get; set; } = "Комментарии";
    public int MaxVisible { get; set; } = 50;
    public bool ShowAuthor { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    /// <summary>Блок CommentsBlock, комментарии которого отображаются.</summary>
    public int? SourceCommentsBlockId { get; set; }

    public PageElement PageElement { get; set; } = null!;
}
