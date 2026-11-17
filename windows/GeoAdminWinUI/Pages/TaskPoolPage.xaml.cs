using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GeoAdminWinUI;

public sealed partial class TaskPoolPage : Page
{
    public TaskPoolPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void SaveInterval_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(IntervalBox.Text.Trim(), out var seconds) || seconds < 1 || seconds > 3600)
        {
            ShowMessage("执行间隔必须是 1-3600 秒", InfoBarSeverity.Error);
            return;
        }

        try
        {
            await AppState.Client.SendAsync<SimpleResult>(
                HttpMethod.Put,
                "/admin/task-settings",
                new TaskIntervalBody { IntervalSeconds = seconds });
            await LoadAsync();
            ShowMessage($"执行间隔已更新为 {seconds} 秒", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var settings = await AppState.Client.GetAsync<TaskSettingsResponse>("/admin/task-settings");
            var tasks = await AppState.Client.GetAsync<QueueTaskList>("/admin/tasks?pageSize=100");

            IntervalBox.Text = settings.IntervalSeconds.ToString();
            StatsText.Text =
                $"待执行 {settings.Stats.Pending} · 执行中 {settings.Stats.Running} · " +
                $"成功 {settings.Stats.Success} · 失败 {settings.Stats.Failed}";
            TaskList.ItemsSource = tasks.Items;
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        MessageBar.Message = message;
        MessageBar.Severity = severity;
        MessageBar.IsOpen = true;
    }
}
