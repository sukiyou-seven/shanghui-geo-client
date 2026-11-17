using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GeoAdminWinUI;

public sealed partial class LocalApiPage : Page
{
    private LocalApiCatalog? catalog;
    private List<LocalApiItem> visibleItems = new();

    public LocalApiPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        MessageBar.IsOpen = false;
        try
        {
            catalog = await AppState.Client.GetAsync<LocalApiCatalog>("/admin/local-apis");
            GroupBox.Items.Clear();
            GroupBox.Items.Add(new ComboBoxItem { Content = "全部分组", Tag = "全部" });
            foreach (var group in catalog.Groups)
            {
                GroupBox.Items.Add(new ComboBoxItem { Content = group.Name, Tag = group.Name });
            }
            GroupBox.SelectedIndex = 0;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message, InfoBarSeverity.Error);
        }
    }

    private void GroupBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        if (catalog is null)
        {
            return;
        }

        var selected = (GroupBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "全部";
        visibleItems = catalog.Groups
            .Where(group => selected == "全部" || group.Name == selected)
            .SelectMany(group => group.Items)
            .ToList();

        LocalApiList.ItemsSource = visibleItems;
        CountText.Text = $"{visibleItems.Count} / {catalog.Total} 个接口";
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (catalog is null)
        {
            ShowMessage("API 清单尚未加载", InfoBarSeverity.Warning);
            return;
        }

        var panel = new StackPanel { Spacing = 6 };
        var boxes = new List<(CheckBox Box, LocalApiGroup Group)>();
        foreach (var group in catalog.Groups)
        {
            var check = new CheckBox
            {
                Content = $"{group.Name}（{group.Items.Count} 个）",
                IsChecked = true,
                Tag = group
            };
            boxes.Add((check, group));
            panel.Children.Add(check);
        }

        var dialog = new ContentDialog
        {
            Title = "导出本程序 API",
            Content = panel,
            PrimaryButtonText = "导出文件",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        var selected = boxes.Where(item => item.Box.IsChecked == true)
            .Select(item => item.Group)
            .ToList();
        if (selected.Count == 0)
        {
            ShowMessage("请至少选择一个分组", InfoBarSeverity.Warning);
            return;
        }

        var content = BuildMarkdown(selected);
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"geoEngine-API-{DateTime.Now:yyyyMMdd-HHmm}"
        };
        picker.FileTypeChoices.Add("Markdown", new List<string> { ".md" });

        if (App.MainWindow is null)
        {
            ShowMessage("无法获取窗口句柄", InfoBarSeverity.Error);
            return;
        }

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        await FileIO.WriteTextAsync(file, content);
        var count = selected.Sum(group => group.Items.Count);
        ShowMessage($"已导出 {count} 个接口到 {file.Name}", InfoBarSeverity.Success);
    }

    private string BuildMarkdown(List<LocalApiGroup> groups)
    {
        var baseUrl = catalog?.BaseUrl ?? AppState.ServerUrl;
        var builder = new StringBuilder();
        builder.AppendLine("# geoEngine API 清单");
        builder.AppendLine();
        builder.AppendLine($"- 服务端：{catalog?.ServerName} v{catalog?.Version}");
        builder.AppendLine($"- Base URL：`{baseUrl}`");
        builder.AppendLine($"- 导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"- 导出范围：{groups.Count} 个分组，{groups.Sum(group => group.Items.Count)} 个接口");
        builder.AppendLine();
        builder.AppendLine("> 本文件由「座天使·文渊」导出。鉴权令牌不会出现在导出内容中。");
        builder.AppendLine();

        foreach (var group in groups)
        {
            builder.AppendLine($"## {group.Name}");
            builder.AppendLine();
            if (!string.IsNullOrWhiteSpace(group.Description))
            {
                builder.AppendLine($"> {group.Description}");
                builder.AppendLine();
            }

            foreach (var item in group.Items)
            {
                builder.AppendLine($"### {item.Name}");
                builder.AppendLine();
                builder.AppendLine($"- 方法：`{item.Method}`");
                builder.AppendLine($"- 地址：`{baseUrl}{item.Path}`");
                if (!string.IsNullOrWhiteSpace(item.Category))
                {
                    builder.AppendLine($"- 分类：{item.Category}");
                }
                if (!string.IsNullOrWhiteSpace(item.ApiCode))
                {
                    builder.AppendLine($"- code：`{item.ApiCode}`");
                }
                builder.AppendLine($"- 鉴权：{item.Auth}");
                builder.AppendLine($"- 状态：{(item.Enabled ? "启用" : "未启用/未发布")}");
                if (item.UpstreamTokenConfigured.HasValue)
                {
                    builder.AppendLine($"- 上游 token：{(item.UpstreamTokenConfigured.Value ? "已配置" : "未配置")}");
                }
                builder.AppendLine($"- 来源：{(item.Source == "api-definition" ? "API 接入配置" : "内置接口")}");
                builder.AppendLine();
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    builder.AppendLine(item.Description);
                    builder.AppendLine();
                }
            }
        }

        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("由 **座天使·文渊** 导出。");
        builder.AppendLine();
        return builder.ToString();
    }

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        MessageBar.Message = message;
        MessageBar.Severity = severity;
        MessageBar.IsOpen = true;
    }
}
