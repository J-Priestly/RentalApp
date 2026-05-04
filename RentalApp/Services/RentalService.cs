using RentalApp.Database.Models;
using RentalApp.Database.States;

namespace RentalApp.Services;

public class RentalService : IRentalService
{
    private readonly IApiService _apiService;

    public RentalService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public decimal CalculateTotalPrice(decimal dailyRate, DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).Days;
        if (days <= 0) days = 1;
        return dailyRate * days;
    }

    public bool IsValidTransition(RentalStatus current, RentalStatus next)
    {
        return (current, next) switch
        {
            (RentalStatus.Requested, RentalStatus.Approved) => true,
            (RentalStatus.Requested, RentalStatus.Rejected) => true,
            (RentalStatus.Approved, RentalStatus.OutForRent) => true,
            (RentalStatus.OutForRent, RentalStatus.Overdue) => true,
            (RentalStatus.OutForRent, RentalStatus.Returned) => true,
            (RentalStatus.Returned, RentalStatus.Completed) => true,
            _ => false
        };
    }

    public async Task<(bool Success, string Message, Rental? Rental)> RequestRentalAsync(
        int itemId, DateTime startDate, DateTime endDate)
    {
        // Validates dates
        if (startDate >= endDate)
            return (false, "End date must be after start date", null);

        if (startDate < DateTime.Today)
            return (false, "Start date cannot be in the past", null);

        // Checks to see if their is a double booking against existing rentals
        var existing = await _apiService.GetOutgoingRentalsAsync();
        var hasOverlap = existing.Any(r =>
            r.ItemId == itemId &&
            r.Status != RentalStatus.Rejected &&
            r.Status != RentalStatus.Completed &&
            startDate < r.EndDate &&
            endDate > r.StartDate);

        if (hasOverlap)
            return (false, "This item is already booked for those dates", null);

        var rental = await _apiService.CreateRentalAsync(itemId, startDate, endDate);

        return rental != null
            ? (true, "Rental request sent!", rental)
            : (false, "Failed to create rental request", null);
    }

    public async Task<(bool Success, string Message)> ApproveRentalAsync(Rental rental)
    {
        try
        {
            var state = RentalStateFactory.GetState(rental.Status);
            state.Approve();
            await _apiService.UpdateRentalStatusAsync(rental.Id, "Approved");
            return (true, "Rental approved");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> RejectRentalAsync(Rental rental)
    {
        try
        {
            var state = RentalStateFactory.GetState(rental.Status);
            state.Reject();
            await _apiService.UpdateRentalStatusAsync(rental.Id, "Rejected");
            return (true, "Rental rejected");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

}
