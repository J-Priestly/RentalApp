using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class CreateItemViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string itemTitle = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string dailyRate = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string latitude = "55.9533";

    [ObservableProperty]
    private string longitude = "-3.1883";

    public CreateItemViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        Title = "List New Item";
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await _apiService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(result);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load categories: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CreateItemAsync()
    {
        if (IsBusy) return;

        // Validate
        if (string.IsNullOrWhiteSpace(ItemTitle))
        { SetError("Title is required"); return; }

        if (string.IsNullOrWhiteSpace(Description))
        { SetError("Description is required"); return; }

        if (!decimal.TryParse(DailyRate, out var rate) || rate <= 0)
        { SetError("Please enter a valid daily rate"); return; }

        if (SelectedCategory == null)
        { SetError("Please select a category"); return; }

        if (!double.TryParse(Latitude, out var lat))
        { SetError("Please enter a valid latitude"); return; }

        if (!double.TryParse(Longitude, out var lng))
        { SetError("Please enter a valid longitude"); return; }

        IsBusy = true;
        ClearError();

        try
        {
            var item = await _apiService.CreateItemAsync(
                ItemTitle, Description, rate,
                SelectedCategory.Id, lat, lng);

            if (item != null)
            {
                await _navigationService.NavigateBackAsync();
            }
            else
            {
                SetError("Failed to create item. Please try again.");
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
    private async Task CancelAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}