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

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    public ItemsListViewModel(IApiService apiService, INavigationService navigationService, IItemRepository itemRepository)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _itemRepository = itemRepository;
        Title = "Browse Items";
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();

        try
        {
            var result = await _apiService.GetItemsAsync();
            Items = new ObservableCollection<Item>(result);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load items: {ex.Message}");
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
    private async Task GoToItemDetailAsync(Item item)
    {
        if (item == null) return;
        await _navigationService.NavigateToAsync($"ItemDetailPage?itemId={item.Id}");
    }
}