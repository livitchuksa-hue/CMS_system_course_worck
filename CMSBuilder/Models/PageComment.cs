namespace CMSBuilder.Models;

public class PageComment
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public int ElementId { get; set; }
    public string AuthorName { get; set; } = "Гость";
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Page Page { get; set; } = null!;
}
