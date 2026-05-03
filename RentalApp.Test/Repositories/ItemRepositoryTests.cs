using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;
using Xunit;

namespace RentalApp.Test.Repositories;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ItemRepository _repository;

    public ItemRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.Seed();
        _repository = new ItemRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAvailableItems()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsItem()
    {
        // Arrange
        var expectedId = 1;

        var result = await _repository.GetByIdAsync(expectedId);

        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("Test Drill", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsItemToDatabase()
    {
        // Arrange
        var newItem = new Item
        {
            Title = "New Ladder",
            Description = "A tall ladder",
            DailyRate = 15.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true,
            Latitude = 55.9533,
            Longitude = -3.1883,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var added = await _repository.AddAsync(newItem);
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Ladder", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesItem_NotReturnedInList()
    {
        await _repository.DeleteAsync(1);
        var allItems = await _repository.GetAllAsync();

        Assert.DoesNotContain(allItems, i => i.Id == 1);
    }
}
