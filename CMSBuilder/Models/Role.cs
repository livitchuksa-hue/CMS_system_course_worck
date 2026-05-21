namespace CMSBuilder.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ICollection<WebsiteUserRole> WebsiteUserRoles { get; set; } = new List<WebsiteUserRole>();
}
