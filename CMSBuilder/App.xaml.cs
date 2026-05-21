using System.Windows;
using CMSBuilder.Data;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Account;

namespace CMSBuilder;

public partial class App : Application
{
    public static NavigationService Navigation { get; } = new();
    public static AuthService Auth { get; } = new();
    public static WebsiteService Websites { get; } = new();
    public static WebsiteMemberService Members { get; } = new();
    public static PageService Pages { get; } = new();
    public static ElementService Elements { get; } = new();
    public static CommentService Comments { get; } = new();
    public static Services.Export.WebsiteExportService Export { get; } = new();
    public static AppSettingsService Settings { get; } = new();
    public static AccountViewModel Account { get; } = new(Navigation, Settings);

    protected override void OnStartup(StartupEventArgs e)
    {
        DbInitializer.Initialize();
        base.OnStartup(e);
    }
}
