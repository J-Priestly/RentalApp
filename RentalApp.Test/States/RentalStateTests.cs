using RentalApp.Database.Models;
using RentalApp.Database.States;
using Xunit;

namespace RentalApp.Test.States;

public class RentalStateTests
{
    // --- factory tests ---

    [Theory]
    [InlineData(RentalStatus.Requested,  typeof(RequestedState))]
    [InlineData(RentalStatus.Approved,   typeof(ApprovedState))]
    [InlineData(RentalStatus.Rejected,   typeof(RejectedState))]
    [InlineData(RentalStatus.OutForRent, typeof(OutForRentState))]
    [InlineData(RentalStatus.Overdue,    typeof(OverdueState))]
    [InlineData(RentalStatus.Returned,   typeof(ReturnedState))]
    [InlineData(RentalStatus.Completed,  typeof(CompletedState))]
    public void Factory_ReturnsCorrectStateType(RentalStatus status, Type expectedType)
    {
        var state = RentalStateFactory.GetState(status);
        Assert.IsType(expectedType, state);
    }

    [Theory]
    [InlineData(RentalStatus.Requested,  "Requested")]
    [InlineData(RentalStatus.Approved,   "Approved")]
    [InlineData(RentalStatus.Rejected,   "Rejected")]
    [InlineData(RentalStatus.OutForRent, "OutForRent")]
    [InlineData(RentalStatus.Overdue,    "Overdue")]
    [InlineData(RentalStatus.Returned,   "Returned")]
    [InlineData(RentalStatus.Completed,  "Completed")]
    public void Factory_StateHasCorrectName(RentalStatus status, string expectedName)
    {
        var state = RentalStateFactory.GetState(status);
        Assert.Equal(expectedName, state.StateName);
    }

    // --- RequestedState ---

    [Fact]
    public void RequestedState_Approve_ReturnsApprovedState()
    {
        var state = new RequestedState();
        var next = state.Approve();
        Assert.IsType<ApprovedState>(next);
    }

    [Fact]
    public void RequestedState_Reject_ReturnsRejectedState()
    {
        var state = new RequestedState();
        var next = state.Reject();
        Assert.IsType<RejectedState>(next);
    }

    [Fact]
    public void RequestedState_MarkOutForRent_ThrowsInvalidOperation()
    {
        var state = new RequestedState();
        Assert.Throws<InvalidOperationException>(() => state.MarkOutForRent());
    }

    [Fact]
    public void RequestedState_MarkReturned_ThrowsInvalidOperation()
    {
        var state = new RequestedState();
        Assert.Throws<InvalidOperationException>(() => state.MarkReturned());
    }

    [Fact]
    public void RequestedState_MarkCompleted_ThrowsInvalidOperation()
    {
        var state = new RequestedState();
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    // --- ApprovedState ---

    [Fact]
    public void ApprovedState_MarkOutForRent_ReturnsOutForRentState()
    {
        var state = new ApprovedState();
        var next = state.MarkOutForRent();
        Assert.IsType<OutForRentState>(next);
    }

    [Fact]
    public void ApprovedState_Approve_ThrowsInvalidOperation()
    {
        var state = new ApprovedState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
    }

    [Fact]
    public void ApprovedState_Reject_ThrowsInvalidOperation()
    {
        var state = new ApprovedState();
        Assert.Throws<InvalidOperationException>(() => state.Reject());
    }

    [Fact]
    public void ApprovedState_MarkCompleted_ThrowsInvalidOperation()
    {
        var state = new ApprovedState();
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    // --- OutForRentState ---

    [Fact]
    public void OutForRentState_MarkReturned_ReturnsReturnedState()
    {
        var state = new OutForRentState();
        var next = state.MarkReturned();
        Assert.IsType<ReturnedState>(next);
    }

    [Fact]
    public void OutForRentState_MarkOverdue_ReturnsOverdueState()
    {
        var state = new OutForRentState();
        var next = state.MarkOverdue();
        Assert.IsType<OverdueState>(next);
    }

    [Fact]
    public void OutForRentState_Approve_ThrowsInvalidOperation()
    {
        var state = new OutForRentState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
    }

    [Fact]
    public void OutForRentState_MarkCompleted_ThrowsInvalidOperation()
    {
        var state = new OutForRentState();
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    // --- OverdueState ---

    [Fact]
    public void OverdueState_MarkReturned_ReturnsReturnedState()
    {
        var state = new OverdueState();
        var next = state.MarkReturned();
        Assert.IsType<ReturnedState>(next);
    }

    [Fact]
    public void OverdueState_Approve_ThrowsInvalidOperation()
    {
        var state = new OverdueState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
    }

    [Fact]
    public void OverdueState_MarkCompleted_ThrowsInvalidOperation()
    {
        var state = new OverdueState();
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    // --- ReturnedState ---

    [Fact]
    public void ReturnedState_MarkCompleted_ReturnsCompletedState()
    {
        var state = new ReturnedState();
        var next = state.MarkCompleted();
        Assert.IsType<CompletedState>(next);
    }

    [Fact]
    public void ReturnedState_Approve_ThrowsInvalidOperation()
    {
        var state = new ReturnedState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
    }

    [Fact]
    public void ReturnedState_MarkReturned_ThrowsInvalidOperation()
    {
        var state = new ReturnedState();
        Assert.Throws<InvalidOperationException>(() => state.MarkReturned());
    }

    // --- terminal states ---

    [Fact]
    public void CompletedState_AllTransitions_ThrowInvalidOperation()
    {
        var state = new CompletedState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
        Assert.Throws<InvalidOperationException>(() => state.Reject());
        Assert.Throws<InvalidOperationException>(() => state.MarkOutForRent());
        Assert.Throws<InvalidOperationException>(() => state.MarkOverdue());
        Assert.Throws<InvalidOperationException>(() => state.MarkReturned());
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    [Fact]
    public void RejectedState_AllTransitions_ThrowInvalidOperation()
    {
        var state = new RejectedState();
        Assert.Throws<InvalidOperationException>(() => state.Approve());
        Assert.Throws<InvalidOperationException>(() => state.Reject());
        Assert.Throws<InvalidOperationException>(() => state.MarkOutForRent());
        Assert.Throws<InvalidOperationException>(() => state.MarkOverdue());
        Assert.Throws<InvalidOperationException>(() => state.MarkReturned());
        Assert.Throws<InvalidOperationException>(() => state.MarkCompleted());
    }

    // --- full workflow chains ---

    [Fact]
    public void FullApprovalWorkflow_TransitionsCorrectly()
    {
        // Requested -> Approved -> OutForRent -> Returned -> Completed
        IRentalState state = new RequestedState();
        state = state.Approve();
        Assert.IsType<ApprovedState>(state);

        state = state.MarkOutForRent();
        Assert.IsType<OutForRentState>(state);

        state = state.MarkReturned();
        Assert.IsType<ReturnedState>(state);

        state = state.MarkCompleted();
        Assert.IsType<CompletedState>(state);
    }

    [Fact]
    public void OverdueWorkflow_TransitionsCorrectly()
    {
        // Requested -> Approved -> OutForRent -> Overdue -> Returned -> Completed
        IRentalState state = new RequestedState();
        state = state.Approve();
        state = state.MarkOutForRent();
        state = state.MarkOverdue();
        Assert.IsType<OverdueState>(state);

        state = state.MarkReturned();
        Assert.IsType<ReturnedState>(state);

        state = state.MarkCompleted();
        Assert.IsType<CompletedState>(state);
    }

    [Fact]
    public void RejectionWorkflow_TransitionsCorrectly()
    {
        // Requested -> Rejected (terminal)
        IRentalState state = new RequestedState();
        state = state.Reject();
        Assert.IsType<RejectedState>(state);

        // can't go anywhere from Rejected
        Assert.Throws<InvalidOperationException>(() => state.Approve());
    }
}
