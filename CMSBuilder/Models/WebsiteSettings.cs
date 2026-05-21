namespace CMSBuilder.Models;

public class WebsiteSettings
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string SiteTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public double PageWidth { get; set; } = 960;
    public double PageHeight { get; set; } = 640;
    public string PageBackgroundColor { get; set; } = "#FFFFFF";

    public Website Website { get; set; } = null!;
}
