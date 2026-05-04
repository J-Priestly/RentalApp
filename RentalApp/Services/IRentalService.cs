using RentalApp.Database.Models;

namespace RentalApp.Services;

public interface IRentalService
{
    decimal CalculateTotalPrice(decimal dailyRate, DateTime startDate, DateTime endDate);
    bool IsValidTransition(RentalStatus current, RentalStatus next);
    Task<(bool Success, string Message, Rental? Rental)> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    Task<(bool Success, string Message)> ApproveRentalAsync(Rental rental);
    Task<(bool Success, string Message)> RejectRentalAsync(Rental rental);
}
