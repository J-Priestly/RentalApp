using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using Xunit;

namespace RentalApp.Test.Services;

public class RentalServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly RentalService _rentalService;

    public RentalServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _rentalService = new RentalService(_mockApiService.Object);
    }

    // --- CalculateTotalPrice ---

    [Fact]
    public void CalculateTotalPrice_WithValidDates_ReturnsCorrectTotal()
    {
        // Sets Up
        var dailyRate = 10.00m;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(3);

        // calls the method
        var result = _rentalService.CalculateTotalPrice(dailyRate, startDate, endDate);

        // checks the result
        Assert.Equal(30.00m, result);
    }

    [Fact]
    public void CalculateTotalPrice_WithSameDates_ReturnsOneDayCharge()
    {
        // Sets Up
        var dailyRate = 10.00m;
        var date = DateTime.Today;

        // calls the method
        var result = _rentalService.CalculateTotalPrice(dailyRate, date, date);

        // checks the result
        Assert.Equal(10.00m, result);
    }

    // --- IsValidTransition ---

    [Theory]
    [InlineData(RentalStatus.Requested, RentalStatus.Approved, true)]
    [InlineData(RentalStatus.Requested, RentalStatus.Rejected, true)]
    [InlineData(RentalStatus.Approved, RentalStatus.OutForRent, true)]
    [InlineData(RentalStatus.OutForRent, RentalStatus.Returned, true)]
    [InlineData(RentalStatus.Returned, RentalStatus.Completed, true)]
    [InlineData(RentalStatus.Rejected, RentalStatus.Approved, false)]
    [InlineData(RentalStatus.Completed, RentalStatus.Requested, false)]
    public void IsValidTransition_ReturnsExpectedResult(
        RentalStatus current, RentalStatus next, bool expected)
    {
        // calls the method
        var result = _rentalService.IsValidTransition(current, next);

        // checks the result
        Assert.Equal(expected, result);
    }

    // --- RequestRentalAsync ---

    [Fact]
    public async Task RequestRentalAsync_WithEndBeforeStart_ReturnsFailure()
    {
        // Sets Up
        var startDate = DateTime.Today.AddDays(3);
        var endDate = DateTime.Today.AddDays(1);

        // calls the method
        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        // checks the result
        Assert.False(success);
        Assert.Contains("End date", message);
        Assert.Null(rental);
    }

    [Fact]
    public async Task RequestRentalAsync_WithPastStartDate_ReturnsFailure()
    {
        // Sets Up
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // calls the method
        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        // checks the result
        Assert.False(success);
        Assert.Contains("past", message);
    }

    [Fact]
    public async Task RequestRentalAsync_WithValidDates_CreatesRental()
    {
        // Sets Up
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        var expectedRental = new Rental { Id = 1, ItemId = 1 };

        _mockApiService
            .Setup(s => s.GetOutgoingRentalsAsync())
            .ReturnsAsync(Enumerable.Empty<Rental>());

        _mockApiService
            .Setup(s => s.CreateRentalAsync(1, startDate, endDate))
            .ReturnsAsync(expectedRental);

        // calls the method
        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        // checks the result
        Assert.True(success);
        Assert.NotNull(rental);
        Assert.Equal(1, rental.Id);
    }

    // --- ApproveRentalAsync ---

    [Fact]
    public async Task ApproveRentalAsync_WhenAlreadyRejected_ReturnsFailure()
    {
        // Sets Up
        var rental = new Rental { Id = 1, Status = RentalStatus.Rejected };

        // calls the method
        var (success, message) = await _rentalService.ApproveRentalAsync(rental);

        // checks the result
        Assert.False(success);
        Assert.Contains("Cannot approve", message);
    }

    [Fact]
    public async Task ApproveRentalAsync_WhenRequested_ReturnsSuccess()
    {
        // Sets Up
        var rental = new Rental { Id = 1, Status = RentalStatus.Requested };

        _mockApiService
            .Setup(s => s.UpdateRentalStatusAsync(1, "Approved"))
            .ReturnsAsync(new ApiStatusResponse { Id = 1, Status = "Approved" });

        // calls the method
        var (success, message) = await _rentalService.ApproveRentalAsync(rental);

        // checks the result
        Assert.True(success);
    }
}
