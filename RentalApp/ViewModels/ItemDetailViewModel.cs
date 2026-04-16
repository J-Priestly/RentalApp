using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Globalization;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly IItemRepository _itemRepository;

    [ObservableProperty]
    private int itemId;

    [ObservableProperty]
    private Item? item;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today.AddDays(3);

    [ObservableProperty]
    private string totalPrice = "0.00";

    [ObservableProperty]
    private bool isOwner;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string editTitle = string.Empty;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private string editDailyRate = string.Empty;

    private readonly IRentalService _rentalService;
    public ItemDetailViewModel(IApiService apiService, INavigationService navigationService, IItemRepository itemRepository, IRentalService rentalService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _itemRepository = itemRepository;
        _rentalService = rentalService;
        Title = "Item Details";
    }

    partial void OnItemIdChanged(int value)
    {
        LoadItemCommand.Execute(null);
    }

    partial void OnStartDateChanged(DateTime value)
    {
        CalculateTotal();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        if (Item == null) return;
        var total = _rentalService.CalculateTotalPrice(Item.DailyRate, StartDate, EndDate);
        TotalPrice = total.ToString("F2");
    }


    [RelayCommand]
    private async Task LoadItemAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();

        try
        {
            Item = await _apiService.GetItemAsync(ItemId);
            if (Item != null)
            {
                Title = Item.Title;
                CalculateTotal();

                // Checks to see if the current user owns this item
                if (_apiService is ApiService api)
                    IsOwner = Item.OwnerId == api.CurrentUserId;

                EditTitle = Item.Title;
                EditDescription = Item.Description;
                EditDailyRate = Item.DailyRate.ToString("F2");
            }
            else
            {
                SetError("Item not found");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load item: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToReviewsAsync()
    {
        if (Item == null) return;
        await _navigationService.NavigateToAsync($"ReviewsPage?itemId={Item.Id}");
    }


    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        if (IsBusy || Item == null) return;
        IsBusy = true;
        ClearError();

        try
        {
            var (success, message, rental) = await _rentalService.RequestRentalAsync(
                ItemId, StartDate, EndDate);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", message, "OK");
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError(message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    [RelayCommand]
    private void StartEditing()
    {
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEditing()
    {
        IsEditing = false;
        if (Item != null)
        {
            EditTitle = Item.Title;
            EditDescription = Item.Description;
            EditDailyRate = Item.DailyRate.ToString("F2");
        }
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (Item == null) return;

        if (string.IsNullOrWhiteSpace(EditTitle))
        { SetError("Title is required"); return; }

        if (!decimal.TryParse(EditDailyRate, out var rate) || rate <= 0)
        { SetError("Please enter a valid daily rate"); return; }

        IsBusy = true;
        ClearError();

        try
        {
            var updated = await _apiService.UpdateItemAsync(
                Item.Id, EditTitle, EditDescription, rate,
                Item.CategoryId, Item.Latitude, Item.Longitude);

            if (updated != null)
            {
                Item = updated;
                Title = updated.Title;
                IsEditing = false;
            }
            else
            {
                SetError("Failed to update item");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    

}