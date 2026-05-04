using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class CreateItemPage : ContentPage
{
    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CreateItemViewModel vm)
            vm.LoadCategoriesCommand.Execute(null);
        await LoadMapAsync();
    }

    private async Task LoadMapAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            LocationMap.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map load failed: {ex.Message}");
        }
    }

    private void OnMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("map://set"))
            return;

        e.Cancel = true;

        var queryString = e.Url.Contains('?') ? e.Url.Split('?')[1] : string.Empty;
        var parameters = queryString.Split('&')
            .Select(p => p.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));

        if (parameters.TryGetValue("lat", out var lat) &&
            parameters.TryGetValue("lng", out var lng) &&
            BindingContext is CreateItemViewModel vm)
        {
            vm.Latitude = lat;
            vm.Longitude = lng;
            vm.AddressDisplay = parameters.TryGetValue("address", out var address)
                ? address
                : "Location selected";
        }
    }
}
