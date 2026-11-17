using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GeoAdminWinUI;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ServerBox.Text = AppState.ServerUrl;
        SessionText.Text = string.IsNullOrWhiteSpace(AppState.SessionToken)
            ? "未登录"
            : "已通过密码 + TOTP 登录";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var changed = !string.Equals(
            AppState.ServerUrl, ServerBox.Text.Trim().TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        SaveSettings();
        if (changed)
        {
            ShowMessage("服务端地址已保存，请重新登录", InfoBarSeverity.Informational);
            if (App.MainWindow is MainWindow main)
            {
                main.ShowLogin();
            }
            return;
        }

        ShowMessage("连接设置已保存", InfoBarSeverity.Success);
    }

    private async void Test_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        try
        {
            var dashboard = await AppState.Client.GetAsync<Dashboard>("/admin/dashboard");
            ShowMessage(
                $"连接成功：{dashboard.Stats.ApiCount} 个接入 · " +
                $"MySQL {(dashboard.Database.Connected ? "在线" : "离线")} · " +
                $"Redis {(dashboard.Redis.Available ? "在线" : "降级")}",
                InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await AppState.Client.SendAsync<SimpleResult>(HttpMethod.Post, "/auth/logout");
        }
        catch
        {
            // 即使服务端不可达，也要清理本机会话
        }

        if (App.MainWindow is MainWindow main)
        {
            main.ShowLogin();
        }
    }

    private async void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await AppState.Client.SendAsync<SimpleResult>(HttpMethod.Post, "/admin/cache/flush");
            ShowMessage("Redis 缓存已清理", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private void SaveSettings()
    {
        AppState.ServerUrl = ServerBox.Text.Trim().TrimEnd('/');
        AppState.Save();
    }

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        MessageBar.Message = message;
        MessageBar.Severity = severity;
        MessageBar.IsOpen = true;
    }
}
