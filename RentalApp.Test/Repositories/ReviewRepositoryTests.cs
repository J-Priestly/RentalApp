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
        // Sets Up
        var added = await AddTestReviewAsync(rating: 5);
        var result = await _repository.GetByIdAsync(added.Id);

        // calls the method
        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal("Test review comment", result.Comment);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReview()
    {
        // Sets Up
        var added = await AddTestReviewAsync(rating: 3);

        // calls the method
        var result = await _repository.GetByIdAsync(added.Id);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal(3, result.Rating);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // calls the method
        var result = await _repository.GetByIdAsync(999);

        // checks the result
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByItemAsync_ReturnsReviewsForItem()
    {
        // Sets Up
        await AddTestReviewAsync(rating: 4, itemId: 1);
        await AddTestReviewAsync(rating: 5, itemId: 1);

        // calls the method
        var result = await _repository.GetByItemAsync(1);

        // checks the result
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, r => Assert.Equal(1, r.ItemId));
    }

    [Fact]
    public async Task GetByItemAsync_WithNoReviews_ReturnsEmpty()
    {
        // calls the method — item 999 has no reviews
        var result = await _repository.GetByItemAsync(999);

        // checks the result
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithReviews_ReturnsCorrectAverage()
    {
        // Sets Up — re-seed to clear previous reviews, then add known ones
        _fixture.Seed();
        await AddTestReviewAsync(rating: 4, itemId: 1);
        await AddTestReviewAsync(rating: 2, itemId: 1);

        // calls the method
        var average = await _repository.GetAverageRatingAsync(1);

        // checks the result
        Assert.Equal(3.0, average);
    }

    [Fact]
    public async Task GetAverageRatingAsync_WithNoReviews_ReturnsZero()
    {
        // calls the method — item 999 has no reviews
        var average = await _repository.GetAverageRatingAsync(999);

        // checks the result
        Assert.Equal(0, average);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllReviews()
    {
        // Sets Up
        await AddTestReviewAsync(rating: 5);

        // calls the method
        var result = await _repository.GetAllAsync();

        // checks the result
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReviewFromDatabase()
    {
        // Sets Up
        var added = await AddTestReviewAsync(rating: 2);

        // calls the method
        await _repository.DeleteAsync(added.Id);
        var result = await _repository.GetByIdAsync(added.Id);

        // checks the result
        Assert.Null(result);
    }
}
