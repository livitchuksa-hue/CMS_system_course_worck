namespace CMSBuilder.Models;

public class Page
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    public bool ShowInNav { get; set; } = true;
    public int SortOrder { get; set; }

    public Website Website { get; set; } = null!;
    public ICollection<PageElement> Elements { get; set; } = new List<PageElement>();
}
