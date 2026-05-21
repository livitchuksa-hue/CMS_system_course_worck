using CMSBuilder.Models;

namespace CMSBuilder.Core;

public static class SessionContext
{
    public static User? CurrentUser { get; set; }
    public static Website? CurrentWebsite { get; set; }
    public static string CurrentUserRole { get; set; } = "Владелец";
    public static string CurrentUserRoleCode { get; set; } = "owner";
}
