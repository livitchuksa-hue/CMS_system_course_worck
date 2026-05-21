namespace CMSBuilder.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ICollection<Website> OwnedWebsites { get; set; } = new List<Website>();
    public ICollection<WebsiteUserRole> WebsiteRoles { get; set; } = new List<WebsiteUserRole>();
}
