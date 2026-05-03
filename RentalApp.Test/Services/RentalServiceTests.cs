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

    [Fact]
    public void CalculateTotalPrice_WithValidDates_ReturnsCorrectTotal()
    {
        // Arrange
        var dailyRate = 10.00m;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(3);

        // Act
        var result = _rentalService.CalculateTotalPrice(dailyRate, startDate, endDate);

        // Assert
        Assert.Equal(30.00m, result);
    }

    [Fact]
    public void CalculateTotalPrice_WithSameDates_ReturnsOneDayCharge()
    {
        // Arrange
        var dailyRate = 10.00m;
        var date = DateTime.Today;

        // Act
        var result = _rentalService.CalculateTotalPrice(dailyRate, date, date);

        // Assert
        Assert.Equal(10.00m, result);
    }

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
        var result = _rentalService.IsValidTransition(current, next);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task RequestRentalAsync_WithEndBeforeStart_ReturnsFailure()
    {
        var startDate = DateTime.Today.AddDays(3);
        var endDate = DateTime.Today.AddDays(1);

        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        Assert.False(success);
        Assert.Contains("End date", message);
        Assert.Null(rental);
    }

    [Fact]
    public async Task RequestRentalAsync_WithPastStartDate_ReturnsFailure()
    {
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        Assert.False(success);
        Assert.Contains("past", message);
    }

    [Fact]
    public async Task RequestRentalAsync_WithValidDates_CreatesRental()
    {
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        var expectedRental = new Rental { Id = 1, ItemId = 1 };

        _mockApiService
            .Setup(s => s.GetOutgoingRentalsAsync())
            .ReturnsAsync(Enumerable.Empty<Rental>());

        _mockApiService
            .Setup(s => s.CreateRentalAsync(1, startDate, endDate))
            .ReturnsAsync(expectedRental);

        var (success, message, rental) = await _rentalService.RequestRentalAsync(1, startDate, endDate);

        Assert.True(success);
        Assert.NotNull(rental);
        Assert.Equal(1, rental.Id);
    }

    [Fact]
    public async Task ApproveRentalAsync_WhenAlreadyRejected_ReturnsFailure()
    {
        var rental = new Rental { Id = 1, Status = RentalStatus.Rejected };

        var (success, message) = await _rentalService.ApproveRentalAsync(rental);

        Assert.False(success);
        Assert.Contains("Cannot approve", message);
    }

    [Fact]
    public async Task ApproveRentalAsync_WhenRequested_ReturnsSuccess()
    {
        var rental = new Rental { Id = 1, Status = RentalStatus.Requested };

        _mockApiService
            .Setup(s => s.UpdateRentalStatusAsync(1, "Approved"))
            .ReturnsAsync(new ApiStatusResponse { Id = 1, Status = "Approved" });

        var (success, message) = await _rentalService.ApproveRentalAsync(rental);

        Assert.True(success);
    }
}
