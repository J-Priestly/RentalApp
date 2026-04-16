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
        // Sets Up — seeded in DatabaseFixture

        // calls the method
        var result = await _repository.GetAllAsync();

        // checks the result
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsItem()
    {
        // Sets Up
        var expectedId = 1;

        // calls the method
        var result = await _repository.GetByIdAsync(expectedId);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("Test Drill", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Sets Up
        var invalidId = 999;

        // calls the method
        var result = await _repository.GetByIdAsync(invalidId);

        // checks the result
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsItemToDatabase()
    {
        // Sets Up
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

        // calls the method
        var added = await _repository.AddAsync(newItem);
        var result = await _repository.GetByIdAsync(added.Id);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal("New Ladder", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesItem_NotReturnedInList()
    {
        // Sets Up — ItemRepository.DeleteAsync sets IsAvailable=false (soft delete)
        var existingId = 1;

        // calls the method
        await _repository.DeleteAsync(existingId);
        var allItems = await _repository.GetAllAsync();

        // checks the result — item no longer appears in the available items list
        Assert.DoesNotContain(allItems, i => i.Id == existingId);
    }
}
