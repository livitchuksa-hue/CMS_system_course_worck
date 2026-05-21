using CMSBuilder.Core;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Base;
using CMSBuilder.Views.Auth;
using CMSBuilder.Views.Dashboard;

namespace CMSBuilder.ViewModels.Auth;

public class RegisterViewModel : BaseViewModel
{
    private readonly NavigationService _navigation;
    private readonly AuthService _auth;
    private readonly AppSettingsService _settings;
    private string _login = string.Empty;
    private string _email = string.Empty;
    private string _error = string.Empty;

    public RegisterViewModel(NavigationService navigation, AuthService auth, AppSettingsService? settings = null)
    {
        _navigation = navigation;
        _auth = auth;
        _settings = settings ?? App.Settings;
        RegisterCommand = new RelayCommand(Register);
        GoLoginCommand = new RelayCommand(() =>
            _navigation.Navigate(new LoginView { DataContext = new LoginViewModel(_navigation, _auth, _settings) }));
    }

    public string LoginName { get => _login; set => SetProperty(ref _login, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string ErrorMessage { get => _error; set => SetProperty(ref _error, value); }

    public RelayCommand RegisterCommand { get; }
    public RelayCommand GoLoginCommand { get; }

    private void Register()
    {
        var (ok, error, user) = _auth.Register(LoginName, Email, Password, ConfirmPassword);
        if (!ok) { ErrorMessage = error; return; }
        SessionContext.CurrentUser = user;
        _navigation.Navigate(new DashboardView
        {
            DataContext = new Dashboard.DashboardViewModel(_navigation, App.Account)
        });
    }
}
