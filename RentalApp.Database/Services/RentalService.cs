using RentalApp.Database.Models;

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
            (RentalStatus.Requested,  RentalStatus.Approved)   => true,
            (RentalStatus.Requested,  RentalStatus.Rejected)   => true,
            (RentalStatus.Approved,   RentalStatus.OutForRent) => true,
            (RentalStatus.OutForRent, RentalStatus.Overdue)    => true,
            (RentalStatus.OutForRent, RentalStatus.Returned)   => true,
            (RentalStatus.Returned,   RentalStatus.Completed)  => true,
            _ => false
        };
    }

    public async Task<(bool Success, string Message, Rental? Rental)> RequestRentalAsync(
        int itemId, DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            return (false, "End date must be after start date", null);

        if (startDate < DateTime.Today)
            return (false, "Start date cannot be in the past", null);

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
        if (!IsValidTransition(rental.Status, RentalStatus.Approved))
            return (false, $"Cannot approve a rental with status '{rental.Status}'");

        var result = await _apiService.UpdateRentalStatusAsync(rental.Id, "Approved");
        return result != null
            ? (true, "Rental approved")
            : (false, "Failed to approve rental");
    }

    public async Task<(bool Success, string Message)> RejectRentalAsync(Rental rental)
    {
        if (!IsValidTransition(rental.Status, RentalStatus.Rejected))
            return (false, $"Cannot reject a rental with status '{rental.Status}'");

        var result = await _apiService.UpdateRentalStatusAsync(rental.Id, "Rejected");
        return result != null
            ? (true, "Rental rejected")
            : (false, "Failed to reject rental");
    }
}
