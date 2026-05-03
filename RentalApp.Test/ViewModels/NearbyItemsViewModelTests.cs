using Moq;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Test.ViewModels;

public class NearbyItemsViewModelTests
{
    // helpers 

    private readonly Mock<ILocationService>  _locationService  = new();
    private readonly Mock<IItemRepository>   _itemRepository   = new();
    private readonly Mock<IApiService>       _apiService       = new();
    private readonly Mock<INavigationService> _navigationService = new();

    private NearbyItemsViewModel CreateViewModel() =>
        new(_locationService.Object,
            _itemRepository.Object,
            _apiService.Object,
            _navigationService.Object);

   
    private static Item MakeItem(int id, string title, double lat, double lng) =>
        new() { Id = id, Title = title, Latitude = lat, Longitude = lng, Category = "Test" };

    // GPS unavailable 

    [Fact]
    public async Task LoadNearbyItems_WhenGpsUnavailable_SetsErrorAndEmptyList()
    {
        // Arrange — GPS returns null (permission denied / hardware off)
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        var vm = CreateViewModel();

        // Act
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HasError);
        Assert.Empty(vm.NearbyItems);
        Assert.Contains("location", vm.LocationStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadNearbyItems_WhenGpsUnavailable_StatusDescribesTheProblem()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Status message telling the user GPS is the problem
        Assert.Contains("unavailable", vm.LocationStatus, StringComparison.OrdinalIgnoreCase);
    }

    // Items within radius 

    [Fact]
    public async Task LoadNearbyItems_WithItemsInRadius_PopulatesNearbyItems()
    {
        // Arrange — Edinburgh city centre
        const double userLat = 55.9533;
        const double userLng = -3.1883;

        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((userLat, userLng));

        // One item ~1 mile away (still in Edinburgh), one ~100 mi away (near Glasgow)
        var nearby  = MakeItem(1, "Drill",  55.9600, -3.2000); 
        var farAway = MakeItem(2, "Ladder", 55.8642, -4.2518); 

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(userLat, userLng, It.IsAny<double>()))
            .ReturnsAsync(new[] { nearby });

        var vm = CreateViewModel();

        // Act
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Assert — only the nearby item should appear
        Assert.Single(vm.NearbyItems);
        Assert.Equal("Drill", vm.NearbyItems[0].Item.Title);
    }

    [Fact]
    public async Task LoadNearbyItems_ItemsAreSortedByDistanceAscending()
    {
        const double userLat = 55.9533;
        const double userLng = -3.1883;

        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((userLat, userLng));

        // Closer item has smaller lat offset
        var closer  = MakeItem(1, "Closer",  55.9560, -3.1900); 
        var further = MakeItem(2, "Further", 55.9700, -3.2100); 

        // Repository returns them in reverse order to verify sorting
        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(userLat, userLng, It.IsAny<double>()))
            .ReturnsAsync(new[] { further, closer });

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.NearbyItems.Count);
        Assert.Equal("Closer",  vm.NearbyItems[0].Item.Title);
        Assert.Equal("Further", vm.NearbyItems[1].Item.Title);
        Assert.True(vm.NearbyItems[0].DistanceMiles < vm.NearbyItems[1].DistanceMiles);
    }

    [Fact]
    public async Task LoadNearbyItems_WithNoItemsInRadius_ShowsEmptyList()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(Enumerable.Empty<Item>());

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Empty(vm.NearbyItems);
        Assert.Contains("No items", vm.LocationStatus, StringComparison.OrdinalIgnoreCase);
    }

    //Status message content

    [Fact]
    public async Task LoadNearbyItems_WhenItemsFound_StatusShowsCount()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new[]
            {
                MakeItem(1, "Tent",   55.9560, -3.190),
                MakeItem(2, "Kayak",  55.9580, -3.195)
            });

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Contains("2", vm.LocationStatus);
    }

    // Repository fallback to API

    [Fact]
    public async Task LoadNearbyItems_WhenRepositoryThrows_FallsBackToApi()
    {
        const double userLat = 55.9533;
        const double userLng = -3.1883;

        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((userLat, userLng));

        // Repository fails (e.g. DB not available)
        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ThrowsAsync(new InvalidOperationException("DB offline"));

        // API returns one item within range
        var apiItem = MakeItem(3, "Hammer", 55.9550, -3.1900);
        _apiService
            .Setup(s => s.GetItemsAsync())
            .ReturnsAsync(new[] { apiItem });

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Should still find the item via the API fallback
        Assert.Single(vm.NearbyItems);
        Assert.Equal("Hammer", vm.NearbyItems[0].Item.Title);
    }

    // DistanceText formatting 

    [Fact]
    public void DistanceText_WhenUnderOnetenth_ShowsYards()
    {
        var display = new NearbyItemDisplay { DistanceMiles = 0.05 };
        Assert.Equal("88yd away", display.DistanceText);
    }

    [Fact]
    public void DistanceText_WhenOverOnetenthMile_ShowsMiles()
    {
        var display = new NearbyItemDisplay { DistanceMiles = 2.7 };
        Assert.Equal("2.7mi away", display.DistanceText);
    }

    // IsBusy flag 

    [Fact]
    public async Task LoadNearbyItems_IsBusyFalseAfterCompletion()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(Enumerable.Empty<Item>());

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task LoadNearbyItems_IsBusyFalseEvenWhenGpsFails()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.False(vm.IsBusy);
    }

    // Radius boundary

    [Fact]
    public async Task LoadNearbyItems_PassesRadiusKmToRepository()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(Enumerable.Empty<Item>());

        var vm = CreateViewModel();
        vm.RadiusMiles = 25;

        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        // Verify the ViewModel passes the configured radius to the repository
        _itemRepository.Verify(
            r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), 25),
            Times.Once);
    }
}
