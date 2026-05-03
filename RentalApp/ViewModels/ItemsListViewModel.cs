using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class ItemsListViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly IItemRepository _itemRepository;

    // Full unfiltered list
    private List<Item> _allItems = new();

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    public ItemsListViewModel(IApiService apiService, INavigationService navigationService, IItemRepository itemRepository)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _itemRepository = itemRepository;
        Title = "Browse Items";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Items = new ObservableCollection<Item>(_allItems);
            return;
        }

        var query = SearchText.Trim().ToLowerInvariant();
        var filtered = _allItems.Where(i =>
            i.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (i.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (i.Category?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

        Items = new ObservableCollection<Item>(filtered);
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ResetError();

        try
        {
            var result = await _itemRepository.GetAllAsync();
            var list = result.ToList();

            // if local DB is empty, pull from API instead
            if (!list.Any())
            {
                var apiItems = await _apiService.GetItemsAsync();
                list = apiItems.ToList();
            }

            _allItems = list;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            // local DB unavailable, try API as fallback
            try
            {
                var apiItems = await _apiService.GetItemsAsync();
                _allItems = apiItems.ToList();
                ApplyFilter();
            }
            catch
            {
                SetError($"Failed to load items: {ex.Message}");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToCreateItemAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    [RelayCommand]
    private async Task GoToNearbyItemsAsync()
    {
        await _navigationService.NavigateToAsync("NearbyItemsPage");
    }

    [RelayCommand]
    private async Task GoToItemDetailAsync(Item item)
    {
        if (item == null) return;
        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }
}
