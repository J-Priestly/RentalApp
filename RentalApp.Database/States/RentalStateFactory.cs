using RentalApp.Database.Models;

namespace RentalApp.Database.States;

public static class RentalStateFactory
{
    public static IRentalState GetState(RentalStatus status) => status switch
    {
        RentalStatus.Requested  => new RequestedState(),
        RentalStatus.Approved   => new ApprovedState(),
        RentalStatus.Rejected   => new RejectedState(),
        RentalStatus.OutForRent => new OutForRentState(),
        RentalStatus.Overdue    => new OverdueState(),
        RentalStatus.Returned   => new ReturnedState(),
        RentalStatus.Completed  => new CompletedState(),
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unknown rental status: {status}")
    };
}
