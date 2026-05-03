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
    private readonly IRentalService _rentalService;

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> incomingRentals = new();

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> outgoingRentals = new();

    [ObservableProperty]
    private bool showingIncoming = true;

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
        ResetError();

        try
        {
            Dictionary<int, string> titleLookup;
            try
            {
                var items = await _apiService.GetItemsAsync();
                titleLookup = items.ToDictionary(i => i.Id, i => i.Title);
            }
            catch
            {
                titleLookup = new Dictionary<int, string>();
            }

            string ResolveTitle(int itemId) =>
                titleLookup.TryGetValue(itemId, out var t) ? t : $"Item #{itemId}";

            IEnumerable<Rental> incoming;
            IEnumerable<Rental> outgoing;

            try
            {
                incoming = await _rentalRepository.GetByItemOwnerAsync(_apiService.CurrentUserId);
                outgoing = await _rentalRepository.GetByBorrowerAsync(_apiService.CurrentUserId);
            }
            catch
            {
                // local DB unavailable, fall back to API
                incoming = await _apiService.GetIncomingRentalsAsync();
                outgoing = await _apiService.GetOutgoingRentalsAsync();
            }

            IncomingRentals = new ObservableCollection<RentalDisplayItem>(
                incoming.Select(r => new RentalDisplayItem
                {
                    Rental = r,
                    ItemTitle = ResolveTitle(r.ItemId)
                }));

            OutgoingRentals = new ObservableCollection<RentalDisplayItem>(
                outgoing.Select(r => new RentalDisplayItem
                {
                    Rental = r,
                    ItemTitle = ResolveTitle(r.ItemId)
                }));
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
    private void ShowIncoming() => ShowingIncoming = true;

    [RelayCommand]
    private void ShowOutgoing() => ShowingIncoming = false;

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
