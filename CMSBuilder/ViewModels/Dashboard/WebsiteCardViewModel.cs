using CMSBuilder.Models;
using CMSBuilder.ViewModels.Base;

namespace CMSBuilder.ViewModels.Dashboard;

public class WebsiteCardViewModel : BaseViewModel
{
    public WebsiteCardViewModel(Website website, string role, string roleCode)
    {
        Website = website;
        Role = role;
        RoleCode = roleCode;
    }

    public Website Website { get; }
    public int Id => Website.Id;
    public string Name => Website.Name;
    public string Role { get; }
    public string RoleCode { get; }
}
