using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;
using Xunit;

namespace RentalApp.Test.Repositories;

public class ReviewRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.Seed();
        _repository = new ReviewRepository(_fixture.Context);
    }

    // helper to avoid repeating the same object setup in every test
    private async Task<Review> AddTestReviewAsync(int rating = 4, int itemId = 1)
    {
        var review = new Review
        {
            ItemId = itemId,
            ReviewerId = 2,
            RentalId = 1,
            Rating = rating,
            Comment = "Test review comment",
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.AddAsync(review);
    }

    [Fact]
    public async Task AddAsync_AddsReviewToDatabase()
    {
        var added = await AddTestReviewAsync(rating: 5);
        var result = await _repository.GetByIdAsync(added.Id);

        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal("Test review comment", result.Comment);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReview()
    {
        // Arrange
        var added = await AddTestReviewAsync(rating: 3);

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Rating);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByItemAsync_ReturnsReviewsForItem()
    {
        await AddTestReviewAsync(rating: 4, itemId: 1);
        await AddTestReviewAsync(rating: 5, itemId: 1);

        var result = await _repository.GetByItemAsync(1);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, r => Assert.Equal(1, r.ItemId));
    }

    [Fact]
    public async Task GetByItemAsync_WithNoReviews_ReturnsEmpty()
    {
        // item 999 has no reviews
        var result = await _repository.GetByItemAsync(999);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithReviews_ReturnsCorrectAverage()
    {
        // re-seed to clear previous reviews, then add two known ones
        _fixture.Seed();
        await AddTestReviewAsync(rating: 4, itemId: 1);
        await AddTestReviewAsync(rating: 2, itemId: 1);

        var average = await _repository.GetAverageRatingAsync(1);

        Assert.Equal(3.0, average);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithNoReviews_ReturnsZero()
    {
        var average = await _repository.GetAverageRatingAsync(999);
        Assert.Equal(0, average);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllReviews()
    {
        await AddTestReviewAsync(rating: 5);
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReviewFromDatabase()
    {
        // Arrange
        var added = await AddTestReviewAsync(rating: 2);

        // Act
        await _repository.DeleteAsync(added.Id);
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        Assert.Null(result);
    }
}
