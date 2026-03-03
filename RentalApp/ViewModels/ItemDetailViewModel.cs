using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;
using System.Globalization;

namespace RentalApp.ViewModels;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class ItemDetailViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

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

    public ItemDetailViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
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
        var days = (EndDate - StartDate).Days;
        if (days <= 0) days = 1;
        var total = Item.DailyRate * days;
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
    private async Task RequestRentalAsync()
    {
        if (IsBusy || Item == null) return;

        if (EndDate <= StartDate)
        {
            SetError("End date must be after start date");
            return;
        }

        IsBusy = true;
        ClearError();

        try
        {
            var rental = await _apiService.CreateRentalAsync(ItemId, StartDate, EndDate);
            if (rental != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Success", "Rental request sent!", "OK");
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError("Failed to create rental request");
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