using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IRentalRepository _rentalRepository;

    [ObservableProperty]
    private ObservableCollection<Rental> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<Rental> outgoingRentals = new();

    [ObservableProperty]
    private bool showingIncoming = true;

    private readonly IRentalService _rentalService;

    public RentalsViewModel(IApiService apiService, IRentalRepository rentalRepository, IRentalService rentalService)
    {
        _apiService = apiService;
        _rentalRepository = rentalRepository;
        _rentalService = rentalService;
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
        var (success, message) = await _rentalService.ApproveRentalAsync(rental);
        if (success)
            await LoadRentalsAsync();
        else
            SetError(message);
    }

    [RelayCommand]
    private async Task RejectRentalAsync(Rental rental)
    {
        if (rental == null) return;
        var (success, message) = await _rentalService.RejectRentalAsync(rental);
        if (success)
            await LoadRentalsAsync();
        else
            SetError(message);
    }


   
}