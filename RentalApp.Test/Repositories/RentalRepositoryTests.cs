using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;
using Xunit;

namespace RentalApp.Test.Repositories;

public class RentalRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly RentalRepository _repository;

    public RentalRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.Seed();
        _repository = new RentalRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRentals()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsRental()
    {
        // Arrange
        var expectedId = 1;

        // Act
        var result = await _repository.GetByIdAsync(expectedId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal(RentalStatus.Requested, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBorrowerAsync_ReturnsBorrowerRentals()
    {
        // seeded rental has BorrowerId = 2
        var result = await _repository.GetByBorrowerAsync(2);

        Assert.NotNull(result);
        Assert.All(result, r => Assert.Equal(2, r.BorrowerId));
    }

    [Fact]
    public async Task GetByBorrowerAsync_WithUnknownBorrower_ReturnsEmpty()
    {
        var result = await _repository.GetByBorrowerAsync(999);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByItemOwnerAsync_ReturnsOwnerRentals()
    {
        // seeded item has OwnerId = 1
        var result = await _repository.GetByItemOwnerAsync(1);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, r => Assert.Equal(1, r.Item!.OwnerId));
    }

    [Fact]
    public async Task AddAsync_AddsRentalToDatabase()
    {
        // Arrange
        var newRental = new Rental
        {
            ItemId = 1,
            BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Status = RentalStatus.Requested,
            TotalPrice = 20.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var added = await _repository.AddAsync(newRental);
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RentalStatus.Requested, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRentalStatus()
    {
        var rental = await _repository.GetByIdAsync(1);
        Assert.NotNull(rental);
        rental.Status = RentalStatus.Approved;

        await _repository.UpdateAsync(rental);
        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(RentalStatus.Approved, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRentalFromDatabase()
    {
        // add a rental first so we don't break other tests
        var rentalToDelete = new Rental
        {
            ItemId = 1,
            BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(12),
            Status = RentalStatus.Requested,
            TotalPrice = 20.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(rentalToDelete);

        await _repository.DeleteAsync(added.Id);
        var result = await _repository.GetByIdAsync(added.Id);

        Assert.Null(result);
    }
}
