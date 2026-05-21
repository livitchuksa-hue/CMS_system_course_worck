namespace CMSBuilder.Models;

public class WebsiteUserRole
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public Website Website { get; set; } = null!;
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
