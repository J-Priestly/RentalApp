namespace RentalApp.Database.States;

public class OverdueState : IRentalState
{
    public string StateName => "Overdue";

    public IRentalState MarkReturned() => new ReturnedState();

    public IRentalState Approve() =>
        throw new InvalidOperationException("Cannot approve an overdue rental.");
    public IRentalState Reject() =>
        throw new InvalidOperationException("Cannot reject an overdue rental.");
    public IRentalState MarkOutForRent() =>
        throw new InvalidOperationException("Item is already out and overdue.");
    public IRentalState MarkOverdue() =>
        throw new InvalidOperationException("Rental is already marked as overdue.");
    public IRentalState MarkCompleted() =>
        throw new InvalidOperationException("Cannot complete — item has not been returned yet.");
}
