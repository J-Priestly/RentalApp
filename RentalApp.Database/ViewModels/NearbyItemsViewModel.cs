using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class NearbyItemsViewModel : BaseViewModel
{
    private readonly ILocationService _locationService;
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<NearbyItemDisplay> nearbyItems = new();

    [ObservableProperty]
    private double radiusMiles = 10;

    [ObservableProperty]
    private string locationStatus = "Tap 'Search Nearby' to find items near you";

    public NearbyItemsViewModel(
        ILocationService locationService,
        IItemRepository itemRepository,
        INavigationService navigationService)
    {
        _locationService = locationService;
        _itemRepository = itemRepository;
        _navigationService = navigationService;
        Title = "Nearby Items";
    }

    [RelayCommand]
    private async Task LoadNearbyItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ResetError();
        LocationStatus = "Getting your location...";

        try
        {
            var position = await _locationService.GetCurrentLocationAsync();

            if (position == null)
            {
                LocationStatus = "Location unavailable — enable GPS and try again";
                SetError("Could not get your location. Please check GPS permissions.");
                return;
            }

            var (lat, lng) = position.Value;
            LocationStatus = $"Searching within {RadiusMiles:F0} miles of your location...";

            var results = await _itemRepository.GetNearbyItemsAsync(lat, lng, RadiusMiles);

            var sorted = results
                .Select(r => new NearbyItemDisplay
                {
                    Item = r.Item,
                    DistanceMiles = r.DistanceMiles
                })
                .ToList();

            NearbyItems = new ObservableCollection<NearbyItemDisplay>(sorted);
            LocationStatus = sorted.Count == 0
                ? $"No items found within {RadiusMiles:F0} miles"
                : $"{sorted.Count} item{(sorted.Count == 1 ? "" : "s")} found within {RadiusMiles:F0} miles";
        }
        catch (Exception ex)
        {
            SetError($"Search failed: {ex.Message}");
            LocationStatus = "Search failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToItemDetailAsync(NearbyItemDisplay display)
    {
        if (display?.Item == null) return;
        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={display.Item.Id}");
    }
}

public class NearbyItemDisplay
{
    public Item Item { get; set; } = null!;
    public double DistanceMiles { get; set; }

    public string DistanceText => DistanceMiles < 0.1
        ? $"{(int)(DistanceMiles * 1760)}yd away"
        : $"{DistanceMiles:F1}mi away";
}
