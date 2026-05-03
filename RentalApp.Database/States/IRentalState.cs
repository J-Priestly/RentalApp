namespace RentalApp.Database.States;
public interface IRentalState
{
    string StateName { get; }
    IRentalState Approve();
    IRentalState Reject();
    IRentalState MarkOutForRent();
    IRentalState MarkOverdue();
    IRentalState MarkReturned();
    IRentalState MarkCompleted();
}
