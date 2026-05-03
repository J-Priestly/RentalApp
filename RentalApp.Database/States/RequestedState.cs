namespace RentalApp.Database.States;

public class RequestedState : IRentalState
{
    public string StateName => "Requested";

    public IRentalState Approve()   => new ApprovedState();
    public IRentalState Reject()    => new RejectedState();

    public IRentalState MarkOutForRent() =>
        throw new InvalidOperationException("Cannot mark as Out for Rent — rental has not been approved yet.");
    public IRentalState MarkOverdue() =>
        throw new InvalidOperationException("Cannot mark as Overdue from Requested state.");
    public IRentalState MarkReturned() =>
        throw new InvalidOperationException("Cannot mark as Returned — item has not been collected yet.");
    public IRentalState MarkCompleted() =>
        throw new InvalidOperationException("Cannot complete a rental that has not started.");
}
