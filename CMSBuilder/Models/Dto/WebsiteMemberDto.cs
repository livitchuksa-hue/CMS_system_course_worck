namespace CMSBuilder.Models.Dto;

public class WebsiteMemberDto
{
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}
