using CMSBuilder.Core;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Base;
using CMSBuilder.Views.Auth;
using CMSBuilder.Views.Dashboard;

namespace CMSBuilder.ViewModels.Auth;

public class LoginViewModel : BaseViewModel
{
    private readonly NavigationService _navigation;
    private readonly AuthService _auth;
    private readonly AppSettingsService _settings;
    private string _login = string.Empty;
    private string _error = string.Empty;
    private bool _rememberMe;

    public LoginViewModel(NavigationService navigation, AuthService auth, AppSettingsService? settings = null)
    {
        _navigation = navigation;
        _auth = auth;
        _settings = settings ?? App.Settings;
        LoginCommand = new RelayCommand(Login);
        GoRegisterCommand = new RelayCommand(() =>
            _navigation.Navigate(new RegisterView { DataContext = new RegisterViewModel(_navigation, _auth, _settings) }));

        var saved = _settings.Load();
        if (saved.RememberMe && !string.IsNullOrEmpty(saved.Login))
        {
            LoginName = saved.Login;
            RememberMe = true;
        }
    }

    public string LoginName
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string ErrorMessage
    {
        get => _error;
        set => SetProperty(ref _error, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public string Password { get; set; } = string.Empty;

    public RelayCommand LoginCommand { get; }
    public RelayCommand GoRegisterCommand { get; }

    private void Login()
    {
        var (ok, error, user) = _auth.Login(LoginName, Password);
        if (!ok)
        {
            ErrorMessage = error;
            return;
        }

        SessionContext.CurrentUser = user;
        if (RememberMe && user != null)
            _settings.Remember(user);
        else
            _settings.Clear();

        ErrorMessage = string.Empty;
        _navigation.Navigate(new DashboardView
        {
            DataContext = new Dashboard.DashboardViewModel(_navigation, App.Account)
        });
    }
}
