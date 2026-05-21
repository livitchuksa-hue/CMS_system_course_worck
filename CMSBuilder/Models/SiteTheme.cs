namespace CMSBuilder.Models;

public class SiteTheme
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string PrimaryColor { get; set; } = "#1B6B3A";
    public string SecondaryColor { get; set; } = "#5C6B64";
    public string AccentColor { get; set; } = "#2E8B57";
    public string FontFamily { get; set; } = "Segoe UI, sans-serif";
    public int ButtonRadius { get; set; } = 8;
    public int Padding { get; set; } = 16;

    public Website Website { get; set; } = null!;
}
