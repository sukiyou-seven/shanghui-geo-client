using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace GeoAdminWinUI;

public sealed partial class ApiPage : Page
{
    private List<ApiDefinition> all = new();

    public ApiPage()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void Search_Click(object sender, RoutedEventArgs e) => ApplyFilter();

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ApplyFilter();
        }
    }

    private async Task LoadAsync()
    {
        ErrorBar.IsOpen = false;
        try
        {
            var result = await AppState.Client.GetAsync<ApiDefinitionList>("/admin/api-definitions?pageSize=200");
            all = result.Items;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorBar.Message = ex.Message;
            ErrorBar.IsOpen = true;
        }
    }

    private void ApplyFilter()
    {
        var term = SearchBox.Text.Trim();
        var items = string.IsNullOrWhiteSpace(term)
            ? all
            : all.Where(item =>
                item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.LocalPath.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.UpstreamPath.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();

        ApiList.ItemsSource = items;
        CountText.Text = $"{items.Count} / {all.Count} 个接口";
    }
}
