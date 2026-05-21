using CMSBuilder.Core;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Auth;
using CMSBuilder.ViewModels.Base;
using CMSBuilder.Views.Auth;

namespace CMSBuilder.ViewModels.Account;

public class AccountViewModel : BaseViewModel
{
    private readonly NavigationService _navigation;
    private readonly AppSettingsService _settings;
    private bool _isOpen;

    public AccountViewModel(NavigationService navigation, AppSettingsService settings)
    {
        _navigation = navigation;
        _settings = settings;
        LogoutCommand = new RelayCommand(Logout);
        ToggleCommand = new RelayCommand(() => IsOpen = !IsOpen);
    }

    public string CurrentUserLogin => SessionContext.CurrentUser?.Login ?? "—";
    public string CurrentUserEmail => SessionContext.CurrentUser?.Email ?? "—";

    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }

    public RelayCommand LogoutCommand { get; }
    public RelayCommand ToggleCommand { get; }

    private void Logout()
    {
        SessionContext.CurrentUser = null;
        SessionContext.CurrentWebsite = null;
        _settings.Clear();
        IsOpen = false;
        _navigation.Navigate(new LoginView
        {
            DataContext = new LoginViewModel(_navigation, App.Auth, _settings)
        });
    }
}
