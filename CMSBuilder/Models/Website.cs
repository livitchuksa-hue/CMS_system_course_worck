namespace CMSBuilder.Models;

public class Website
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastExportPath { get; set; }
    public DateTime? LastExportedAt { get; set; }

    public User Owner { get; set; } = null!;
    public WebsiteSettings? Settings { get; set; }
    public SiteTheme? Theme { get; set; }
    public ICollection<Page> Pages { get; set; } = new List<Page>();
    public ICollection<WebsiteUserRole> UserRoles { get; set; } = new List<WebsiteUserRole>();
    public ICollection<ElementAction> Actions { get; set; } = new List<ElementAction>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<VisitStatistic> VisitStatistics { get; set; } = new List<VisitStatistic>();
}
