namespace RentalApp.Database.States;

public class ApprovedState : IRentalState
{
    public string StateName => "Approved";

    public IRentalState MarkOutForRent() => new OutForRentState();

    public IRentalState Approve() =>
        throw new InvalidOperationException("Rental is already approved.");
    public IRentalState Reject() =>
        throw new InvalidOperationException("Cannot reject a rental that has already been approved.");
    public IRentalState MarkOverdue() =>
        throw new InvalidOperationException("Cannot mark as Overdue — item has not been collected yet.");
    public IRentalState MarkReturned() =>
        throw new InvalidOperationException("Cannot mark as Returned — item has not been collected yet.");
    public IRentalState MarkCompleted() =>
        throw new InvalidOperationException("Cannot complete a rental that has not started.");
}
