using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GeoAdminWinUI;

public sealed partial class LogsPage : Page
{
    public LogsPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            var result = await AppState.Client.GetAsync<LogListResult>("/admin/logs?page=1&pageSize=100");
            LogList.ItemsSource = result.Items;
            MessageBar.IsOpen = false;
        }
        catch (Exception ex)
        {
            MessageBar.Message = ex.Message;
            MessageBar.Severity = InfoBarSeverity.Error;
            MessageBar.IsOpen = true;
        }
    }
}
