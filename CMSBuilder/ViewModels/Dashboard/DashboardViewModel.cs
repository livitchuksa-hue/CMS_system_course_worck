using System.Collections.ObjectModel;
using CMSBuilder.Core;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Account;
using CMSBuilder.ViewModels.Base;
using CMSBuilder.ViewModels.Editor;
using CMSBuilder.Views.Editor;

namespace CMSBuilder.ViewModels.Dashboard;

public class DashboardViewModel : BaseViewModel
{
    private readonly NavigationService _navigation;
    private readonly WebsiteService _websiteService;
    private readonly AccountViewModel _account;
    private string _searchText = string.Empty;

    public DashboardViewModel(NavigationService navigation, AccountViewModel? account = null)
    {
        _navigation = navigation;
        _websiteService = App.Websites;
        _account = account ?? App.Account;
        Websites = new ObservableCollection<WebsiteCardViewModel>();
        CreateWebsiteCommand = new RelayCommand(CreateWebsite);
        OpenWebsiteCommand = new RelayCommand<WebsiteCardViewModel>(OpenWebsite);
        OpenAccountCommand = _account.ToggleCommand;
        Load();
    }

    public AccountViewModel Account => _account;
    public ObservableCollection<WebsiteCardViewModel> Websites { get; }

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }

    public RelayCommand CreateWebsiteCommand { get; }
    public RelayCommand<WebsiteCardViewModel> OpenWebsiteCommand { get; }
    public RelayCommand OpenAccountCommand { get; }

    private void Load()
    {
        if (SessionContext.CurrentUser == null) return;
        Websites.Clear();
        var list = _websiteService.GetUserWebsites(SessionContext.CurrentUser.Id);
        if (!string.IsNullOrWhiteSpace(SearchText))
            list = list.Where(w => w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var w in list)
        {
            var roleCode = _websiteService.GetUserRoleCode(SessionContext.CurrentUser.Id, w);
            var role = WebsiteService.GetUserRoleNameFromCode(roleCode);
            Websites.Add(new WebsiteCardViewModel(w, role, roleCode));
        }
    }

    private void CreateWebsite()
    {
        if (SessionContext.CurrentUser == null) return;
        var site = _websiteService.CreateWebsite(SessionContext.CurrentUser.Id, $"Сайт {DateTime.Now:dd.MM HH:mm}");
        OpenWebsite(new WebsiteCardViewModel(site, "Владелец", "owner"));
    }

    private void OpenWebsite(WebsiteCardViewModel? card)
    {
        if (card == null) return;
        _account.IsOpen = false;
        SessionContext.CurrentWebsite = card.Website;
        SessionContext.CurrentUserRole = card.Role;
        SessionContext.CurrentUserRoleCode = card.RoleCode;
        _navigation.Navigate(new WebsiteEditorView
        {
            DataContext = new WebsiteEditorViewModel(_navigation, card.Website.Id)
        });
    }
}
