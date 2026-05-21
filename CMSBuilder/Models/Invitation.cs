namespace CMSBuilder.Models;

public class Invitation
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string InvitedLogin { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAccepted { get; set; }

    public Website Website { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
