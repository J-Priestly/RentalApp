using System.Globalization;
using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class ItemDetailPage : ContentPage
{
    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (BindingContext is not ItemDetailViewModel vm) return;

        // Item just loaded — show read-only location map
        if (e.PropertyName == nameof(ItemDetailViewModel.Item) && vm.Item != null)
        {
            await LoadViewMapAsync(vm.Item.Latitude, vm.Item.Longitude);
        }

        // Edit mode activated — load interactive map centred on current location
        if (e.PropertyName == nameof(ItemDetailViewModel.IsEditing) && vm.IsEditing && vm.Item != null)
        {
            await LoadEditMapAsync(vm.Item.Latitude, vm.Item.Longitude);
        }
    }

    private async Task LoadViewMapAsync(double lat, double lng)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("view-map.html");
            using var reader = new StreamReader(stream);
            var html = (await reader.ReadToEndAsync())
                .Replace("PLACEHOLDER_LAT", lat.ToString("F6", CultureInfo.InvariantCulture))
                .Replace("PLACEHOLDER_LNG", lng.ToString("F6", CultureInfo.InvariantCulture));
            ViewMap.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ViewMap load failed: {ex.Message}");
        }
    }

    private async Task LoadEditMapAsync(double lat, double lng)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("map.html");
            using var reader = new StreamReader(stream);
            var latStr = lat.ToString("F6", CultureInfo.InvariantCulture);
            var lngStr = lng.ToString("F6", CultureInfo.InvariantCulture);
            var html = (await reader.ReadToEndAsync())
                // Centre the map on the item's location
                .Replace("55.9533, -3.1883", $"{latStr}, {lngStr}")
                // Pre-place a draggable marker without triggering sendToMaui
                .Replace("// CALL_INIT_PLACEHOLDER", $"setInitialPosition({latStr}, {lngStr});");
            EditMap.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EditMap load failed: {ex.Message}");
        }
    }

    private void OnEditMapNavigating(object sender, WebNavigatingEventArgs e)
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
            BindingContext is ItemDetailViewModel vm)
        {
            vm.EditLatitude = lat;
            vm.EditLongitude = lng;
            vm.EditAddressDisplay = parameters.TryGetValue("address", out var address)
                ? address
                : "Location selected";
        }
    }
}
