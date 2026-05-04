namespace RentalApp.Database.States;

public class OutForRentState : IRentalState
{
    public string StateName => "OutForRent";

    public IRentalState MarkOverdue()  => new OverdueState();
    public IRentalState MarkReturned() => new ReturnedState();

    public IRentalState Approve() =>
        throw new InvalidOperationException("Cannot approve — rental is already active.");
    public IRentalState Reject() =>
        throw new InvalidOperationException("Cannot reject — item is already out for rent.");
    public IRentalState MarkOutForRent() =>
        throw new InvalidOperationException("Item is already out for rent.");
    public IRentalState MarkCompleted() =>
        throw new InvalidOperationException("Cannot complete — item has not been returned yet.");
}
