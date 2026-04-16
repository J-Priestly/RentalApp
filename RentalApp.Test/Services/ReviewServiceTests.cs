using Moq;
using RentalApp.Database.Models;
using RentalApp.Services;
using Xunit;

namespace RentalApp.Test.Services;

public class ReviewServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _reviewService = new ReviewService(_mockApiService.Object);
    }

    // --- IsValidRating ---

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    public void IsValidRating_ReturnsExpectedResult(int rating, bool expected)
    {
        // calls the method
        var result = _reviewService.IsValidRating(rating);

        // checks the result
        Assert.Equal(expected, result);
    }

    // --- CalculateAverageRating ---

    [Fact]
    public void CalculateAverageRating_WithReviews_ReturnsCorrectAverage()
    {
        // Sets Up
        var reviews = new List<Review>
        {
            new Review { Rating = 4 },
            new Review { Rating = 5 },
            new Review { Rating = 3 }
        };

        // calls the method
        var result = _reviewService.CalculateAverageRating(reviews);

        // checks the result
        Assert.Equal(4.0, result);
    }

    [Fact]
    public void CalculateAverageRating_WithNoReviews_ReturnsZero()
    {
        // calls the method
        var result = _reviewService.CalculateAverageRating(new List<Review>());

        // checks the result
        Assert.Equal(0, result);
    }

    // --- SubmitReviewAsync ---

    [Fact]
    public async Task SubmitReviewAsync_WithInvalidRating_ReturnsFailure()
    {
        // Sets Up & calls the method
        var (success, message, review) = await _reviewService.SubmitReviewAsync(1, 1, 0, "Great!");

        // checks the result
        Assert.False(success);
        Assert.Contains("Rating must be between 1 and 5", message);
        Assert.Null(review);
    }

    [Fact]
    public async Task SubmitReviewAsync_WithEmptyComment_ReturnsFailure()
    {
        // Sets Up & calls the method
        var (success, message, review) = await _reviewService.SubmitReviewAsync(1, 1, 5, "");

        // checks the result
        Assert.False(success);
        Assert.Contains("comment", message);
        Assert.Null(review);
    }

    [Fact]
    public async Task SubmitReviewAsync_WithValidData_ReturnsSuccess()
    {
        // Sets Up
        var expectedReview = new Review { Id = 1, ItemId = 1, Rating = 5 };

        _mockApiService
            .Setup(s => s.CreateReviewAsync(1, 1, 5, "Great item!"))
            .ReturnsAsync(expectedReview);

        // calls the method
        var (success, message, review) = await _reviewService.SubmitReviewAsync(1, 1, 5, "Great item!");

        // checks the result
        Assert.True(success);
        Assert.NotNull(review);
        Assert.Equal(5, review.Rating);
    }
}
