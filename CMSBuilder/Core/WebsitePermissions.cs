namespace CMSBuilder.Core;

public static class WebsitePermissions
{
    public static string NormalizeRoleCode(string? roleNameOrCode)
    {
        if (string.IsNullOrWhiteSpace(roleNameOrCode)) return "viewer";
        var v = roleNameOrCode.Trim().ToLowerInvariant();
        return v switch
        {
            "владелец" or "owner" => "owner",
            "администратор" or "admin" => "admin",
            "модератор" or "moderator" => "moderator",
            "просмотр" or "viewer" => "viewer",
            _ => v
        };
    }

    public static bool CanEditCanvas(string roleCode) =>
        roleCode is "owner" or "admin" or "moderator";

    public static bool CanManageSettings(string roleCode) =>
        roleCode is "owner" or "admin";

    public static bool CanManageTeam(string roleCode) =>
        roleCode is "owner" or "admin";

    public static bool CanSaveProject(string roleCode) =>
        CanEditCanvas(roleCode);

    public static bool CanLaunchSite(string roleCode) => true;
}
