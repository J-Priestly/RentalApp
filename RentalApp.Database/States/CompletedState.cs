namespace RentalApp.Database.States;

public class CompletedState : IRentalState
{
    public string StateName => "Completed";

    public IRentalState Approve()        => throw new InvalidOperationException("Rental is already completed.");
    public IRentalState Reject()         => throw new InvalidOperationException("Rental is already completed.");
    public IRentalState MarkOutForRent() => throw new InvalidOperationException("Rental is already completed.");
    public IRentalState MarkOverdue()    => throw new InvalidOperationException("Rental is already completed.");
    public IRentalState MarkReturned()   => throw new InvalidOperationException("Rental is already completed.");
    public IRentalState MarkCompleted()  => throw new InvalidOperationException("Rental is already completed.");
}
