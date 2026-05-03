namespace RentalApp.Database.States;

public class RejectedState : IRentalState
{
    public string StateName => "Rejected";

    public IRentalState Approve()        => throw new InvalidOperationException("Cannot approve a rejected rental.");
    public IRentalState Reject()         => throw new InvalidOperationException("Rental is already rejected.");
    public IRentalState MarkOutForRent() => throw new InvalidOperationException("Cannot mark rejected rental as out for rent.");
    public IRentalState MarkOverdue()    => throw new InvalidOperationException("Cannot mark rejected rental as overdue.");
    public IRentalState MarkReturned()   => throw new InvalidOperationException("Cannot mark rejected rental as returned.");
    public IRentalState MarkCompleted()  => throw new InvalidOperationException("Cannot complete a rejected rental.");
}
