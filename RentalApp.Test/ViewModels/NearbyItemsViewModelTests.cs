using Moq;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using RentalApp.ViewModels;
using Xunit;

namespace RentalApp.Test.ViewModels;

public class NearbyItemsViewModelTests
{
    private readonly Mock<ILocationService>   _locationService   = new();
    private readonly Mock<IItemRepository>    _itemRepository    = new();
    private readonly Mock<INavigationService> _navigationService = new();
    private readonly Mock<IApiService>        _apiService        = new();

    private NearbyItemsViewModel CreateViewModel() =>
        new(_locationService.Object,
            _itemRepository.Object,
            _navigationService.Object,
            _apiService.Object);

    private static NearbyItemResult MakeResult(int id, string title, double distanceMiles) =>
        new()
        {
            Item = new Item { Id = id, Title = title, Category = "Test" },
            DistanceMiles = distanceMiles
        };

    // GPS unavailable

    [Fact]
    public async Task LoadNearbyItems_WhenGpsUnavailable_SetsErrorAndEmptyList()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

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

        Assert.Contains("unavailable", vm.LocationStatus, StringComparison.OrdinalIgnoreCase);
    }

    // Items within radius

    [Fact]
    public async Task LoadNearbyItems_WithItemsInRadius_PopulatesNearbyItems()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new[] { MakeResult(1, "Drill", 0.8) });

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Single(vm.NearbyItems);
        Assert.Equal("Drill", vm.NearbyItems[0].Item.Title);
    }

    [Fact]
    public async Task LoadNearbyItems_ItemsAreSortedByDistanceAscending()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        // Repository returns pre-sorted results (PostGIS handles ordering)
        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new[]
            {
                MakeResult(1, "Closer",  0.5),
                MakeResult(2, "Further", 2.1)
            });

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
            .ReturnsAsync(Enumerable.Empty<NearbyItemResult>());

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Empty(vm.NearbyItems);
        Assert.Contains("No items", vm.LocationStatus, StringComparison.OrdinalIgnoreCase);
    }

    // Status message content

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
                MakeResult(1, "Tent",  0.4),
                MakeResult(2, "Kayak", 1.2)
            });

        var vm = CreateViewModel();
        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        Assert.Contains("2", vm.LocationStatus);
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
            .ReturnsAsync(Enumerable.Empty<NearbyItemResult>());

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
    public async Task LoadNearbyItems_PassesRadiusToRepository()
    {
        _locationService
            .Setup(s => s.GetCurrentLocationAsync())
            .ReturnsAsync((55.9533, -3.1883));

        _itemRepository
            .Setup(r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(Enumerable.Empty<NearbyItemResult>());

        var vm = CreateViewModel();
        vm.RadiusMiles = 25;

        await vm.LoadNearbyItemsCommand.ExecuteAsync(null);

        _itemRepository.Verify(
            r => r.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), 25),
            Times.Once);
    }
}
