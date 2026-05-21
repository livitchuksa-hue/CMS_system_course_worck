using System.Windows;
using CMSBuilder.Core;
using CMSBuilder.ViewModels.Auth;
using CMSBuilder.ViewModels.Dashboard;
using CMSBuilder.Views.Auth;
using CMSBuilder.Views.Dashboard;

namespace CMSBuilder;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        App.Navigation.SetFrame(MainFrame);
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var user = App.Settings.TryRestoreUser();
        if (user != null)
        {
            SessionContext.CurrentUser = user;
            MainFrame.Navigate(new DashboardView
            {
                DataContext = new DashboardViewModel(App.Navigation, App.Account)
            });
            return;
        }

        MainFrame.Navigate(new LoginView
        {
            DataContext = new LoginViewModel(App.Navigation, App.Auth, App.Settings)
        });
    }
}
