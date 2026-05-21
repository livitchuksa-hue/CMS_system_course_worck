namespace CMSBuilder.Models;

public class VisitStatistic
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public DateTime Date { get; set; }
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }

    public Website Website { get; set; } = null!;
}
