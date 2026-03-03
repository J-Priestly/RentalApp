using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    [ObservableProperty]
    private bool showingIncoming = true;

    public RentalsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "My Rentals";
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();

        try
        {
            var incoming = await _apiService.GetIncomingRentalsAsync();
            IncomingRentals = new ObservableCollection<Rental>(incoming);

            var outgoing = await _apiService.GetOutgoingRentalsAsync();
            OutgoingRentals = new ObservableCollection<Rental>(outgoing);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ShowIncoming()
    {
        ShowingIncoming = true;
    }

    [RelayCommand]
    private void ShowOutgoing()
    {
        ShowingIncoming = false;
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(Rental rental)
    {
        if (rental == null) return;
        await UpdateStatusAsync(rental.Id, "Approved");
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        if (rental == null) return;
        await UpdateStatusAsync(rental.Id, "Rejected");
    }

    private async Task UpdateStatusAsync(int rentalId, string status)
    {
        try
        {
            var result = await _apiService.UpdateRentalStatusAsync(rentalId, status);
            if (result != null)
            {
                await LoadRentalsAsync();
            }
            else
            {
                SetError($"Failed to update status to {status}");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
    }
}