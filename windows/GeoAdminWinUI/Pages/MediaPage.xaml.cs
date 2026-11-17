using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace GeoAdminWinUI;

public sealed partial class MediaPage : Page
{
    private List<MediaItem> items = new();

    public MediaPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Query_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        MessageBar.IsOpen = false;
        try
        {
            var query = new List<string> { "page=1", "pageSize=100" };
            var term = SearchBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(term))
            {
                query.Add("keyword=" + Uri.EscapeDataString(term));
            }

            var type = SelectedTag(TypeBox);
            if (!string.IsNullOrWhiteSpace(type))
            {
                query.Add("mediaType=" + Uri.EscapeDataString(type));
            }

            var status = SelectedTag(StatusBox);
            if (!string.IsNullOrWhiteSpace(status))
            {
                query.Add("publishStatus=" + Uri.EscapeDataString(status));
            }

            var result = await AppState.Client.GetAsync<MediaListResult>(
                "/admin/media?" + string.Join("&", query));
            items = result.Items;
            MediaList.ItemsSource = items;
            CountText.Text = $"{result.Total} 条媒体，显示 {items.Count} 条";
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private async void Sync_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await AppState.Client.SendAsync<SimpleResult>(
                HttpMethod.Post,
                "/admin/sync/run",
                new SyncRequestBody
                {
                    Sources = new List<string> { "media-list", "media-dy-list", "media-xhs-list" },
                    MaxPages = 5,
                    Full = true
                });
            ShowMessage("媒体同步已进入任务池，可在「任务池」查看执行进度。", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private async void MediaAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int id)
        {
            return;
        }

        var item = items.FirstOrDefault(value => value.Id == id);
        if (item is null)
        {
            return;
        }

        try
        {
            var publish = item.PublishStatus != "published";
            await AppState.Client.SendAsync<SimpleResult>(
                HttpMethod.Post,
                publish ? "/admin/media/publish" : "/admin/media/offline",
                new PublishBody { Ids = new List<int> { id } });
            await LoadAsync();
            ShowMessage(publish ? "媒体已发布" : "媒体已下架", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private static string SelectedTag(ComboBox box)
        => (box.SelectedItem as ComboBoxItem)?.Tag as string ?? "";

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        MessageBar.Message = message;
        MessageBar.Severity = severity;
        MessageBar.IsOpen = true;
    }
}
