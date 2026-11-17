using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace GeoAdminWinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "座天使·文渊";
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 840));
        LoginServerBox.Text = AppState.ServerUrl;
        LoginOverlay.Visibility = Visibility.Visible;
        Nav.Visibility = Visibility.Collapsed;
        LoginPasswordBox.Focus(FocusState.Programmatic);
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Type? pageType;
        if (args.IsSettingsSelected)
        {
            pageType = typeof(SettingsPage);
        }
        else if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            pageType = tag switch
            {
                "dashboard" => typeof(DashboardPage),
                "apis" => typeof(ApiPage),
                "media" => typeof(MediaPage),
                "tasks" => typeof(TaskPoolPage),
                "localapis" => typeof(LocalApiPage),
                "logs" => typeof(LogsPage),
                _ => typeof(DashboardPage)
            };
        }
        else
        {
            pageType = null;
        }

        if (pageType is not null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e) => await LoginAsync();

    private async void LoginInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await LoginAsync();
        }
    }

    private async Task LoginAsync()
    {
        var server = LoginServerBox.Text.Trim();
        var password = LoginPasswordBox.Password;
        var totp = LoginTotpBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(server))
        {
            ShowLoginError("请填写服务端地址");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowLoginError("请输入登录密码");
            return;
        }

        if (totp.Length != 6 || !totp.All(char.IsDigit))
        {
            ShowLoginError("请输入 6 位 TOTP 验证码");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginErrorBar.IsOpen = false;

        try
        {
            AppState.ServerUrl = server.TrimEnd('/');
            AppState.Save();
            var response = await AppState.LoginClient.SendAsync<LoginResponse>(
                HttpMethod.Post,
                "/auth/login",
                new LoginRequest { Password = password, Totp = totp });

            if (string.IsNullOrWhiteSpace(response.Token))
            {
                throw new ApiException("登录失败：服务端未返回会话令牌");
            }

            AppState.SessionToken = response.Token;
            LoginPasswordBox.Password = "";
            LoginTotpBox.Text = "";
            LoginOverlay.Visibility = Visibility.Collapsed;
            Nav.Visibility = Visibility.Visible;
            Nav.SelectedItem = Nav.MenuItems[0];
            if (ContentFrame.CurrentSourcePageType != typeof(DashboardPage))
            {
                ContentFrame.Navigate(typeof(DashboardPage), null, new EntranceNavigationTransitionInfo());
            }
        }
        catch (Exception ex)
        {
            ShowLoginError(ex.Message);
            LoginTotpBox.Text = "";
            LoginTotpBox.Focus(FocusState.Programmatic);
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    public void ShowLogin()
    {
        AppState.SessionToken = "";
        LoginPasswordBox.Password = "";
        LoginTotpBox.Text = "";
        LoginServerBox.Text = AppState.ServerUrl;
        LoginErrorBar.IsOpen = false;
        ContentFrame.Content = null;
        Nav.Visibility = Visibility.Collapsed;
        LoginOverlay.Visibility = Visibility.Visible;
        LoginPasswordBox.Focus(FocusState.Programmatic);
    }

    private void ShowLoginError(string message)
    {
        LoginErrorBar.Message = message;
        LoginErrorBar.IsOpen = true;
    }
}
