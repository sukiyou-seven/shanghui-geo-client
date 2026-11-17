using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GeoAdminWinUI;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        LoadingRing.IsActive = true;
        ErrorBar.IsOpen = false;

        try
        {
            var dashboard = await AppState.Client.GetAsync<Dashboard>("/admin/dashboard");
            var stats = dashboard.Stats;

            ApiCountText.Text = stats.ApiCount.ToString();
            ApiSubText.Text = $"已启用 {stats.EnabledApiCount} · 已配 token {stats.TokenConfiguredCount}";
            MediaCountText.Text = stats.MediaCount.ToString();
            MediaSubText.Text = $"已发布 {stats.PublishedMediaCount} 条";
            RequestCountText.Text = stats.RequestCount24h.ToString();
            RequestSubText.Text = $"成功率 {UiText.Percent(stats.SuccessRate)} · 平均 {stats.AvgDurationMs:F0} ms";
            TaskPendingText.Text = (dashboard.TaskPool?.Pending ?? 0).ToString();
            TaskSubText.Text = $"执行中 {dashboard.TaskPool?.Running ?? 0} · 间隔 {dashboard.TaskPool?.IntervalSeconds ?? 0} 秒";

            ServiceText.Text =
                $"MySQL  {(dashboard.Database.Connected ? "在线" : "离线")}   {dashboard.Database.Display}\n" +
                $"Redis   {(dashboard.Redis.Available ? "在线" : "降级")}   db {dashboard.Redis.Database} · {dashboard.Redis.LatencyMs} ms\n" +
                $"管理令牌 {(dashboard.Gateway.AdminTokenConfigured ? "已设置" : "未设置")} · " +
                $"网关令牌 {(dashboard.Gateway.GatewayKeyConfigured ? "已启用" : "未启用")}\n" +
                $"同步：{dashboard.Sync.Message}";

            SyncText.Text = dashboard.TaskPool is null
                ? "任务池状态暂不可用"
                : $"任务池：待执行 {dashboard.TaskPool.Pending} · 执行中 {dashboard.TaskPool.Running} · " +
                  $"成功 {dashboard.TaskPool.Success} · 失败 {dashboard.TaskPool.Failed}";
        }
        catch (Exception ex)
        {
            ErrorBar.Message = ex.Message;
            ErrorBar.IsOpen = true;
        }
        finally
        {
            LoadingRing.IsActive = false;
        }
    }
}
