namespace RentalApp.Database.States;

public class ReturnedState : IRentalState
{
    public string StateName => "Returned";

    public IRentalState MarkCompleted() => new CompletedState();

    public IRentalState Approve() =>
        throw new InvalidOperationException("Cannot approve — item has already been returned.");
    public IRentalState Reject() =>
        throw new InvalidOperationException("Cannot reject — item has already been returned.");
    public IRentalState MarkOutForRent() =>
        throw new InvalidOperationException("Item has already been returned.");
    public IRentalState MarkOverdue() =>
        throw new InvalidOperationException("Item has already been returned.");
    public IRentalState MarkReturned() =>
        throw new InvalidOperationException("Item is already marked as returned.");
}
