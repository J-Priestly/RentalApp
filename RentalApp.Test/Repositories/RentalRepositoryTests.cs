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
        // Sets Up — seeded in DatabaseFixture

        // calls the method
        var result = await _repository.GetAllAsync();

        // checks the result
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsRental()
    {
        // Sets Up
        var expectedId = 1;

        // calls the method
        var result = await _repository.GetByIdAsync(expectedId);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal(RentalStatus.Requested, result.Status);
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
    public async Task GetByBorrowerAsync_ReturnsBorrowerRentals()
    {
        // Sets Up — seeded rental has BorrowerId = 2
        var borrowerId = 2;

        // calls the method
        var result = await _repository.GetByBorrowerAsync(borrowerId);

        // checks the result
        Assert.NotNull(result);
        Assert.All(result, r => Assert.Equal(borrowerId, r.BorrowerId));
    }

    [Fact]
    public async Task GetByBorrowerAsync_WithUnknownBorrower_ReturnsEmpty()
    {
        // Sets Up
        var unknownBorrowerId = 999;

        // calls the method
        var result = await _repository.GetByBorrowerAsync(unknownBorrowerId);

        // checks the result
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByItemOwnerAsync_ReturnsOwnerRentals()
    {
        // Sets Up — seeded item has OwnerId = 1
        var ownerId = 1;

        // calls the method
        var result = await _repository.GetByItemOwnerAsync(ownerId);

        // checks the result
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, r => Assert.Equal(ownerId, r.Item!.OwnerId));
    }

    [Fact]
    public async Task AddAsync_AddsRentalToDatabase()
    {
        // Sets Up
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

        // calls the method
        var added = await _repository.AddAsync(newRental);
        var result = await _repository.GetByIdAsync(added.Id);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal(RentalStatus.Requested, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRentalStatus()
    {
        // Sets Up
        var rental = await _repository.GetByIdAsync(1);
        Assert.NotNull(rental);
        rental.Status = RentalStatus.Approved;

        // calls the method
        await _repository.UpdateAsync(rental);
        var result = await _repository.GetByIdAsync(1);

        // checks the result
        Assert.NotNull(result);
        Assert.Equal(RentalStatus.Approved, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRentalFromDatabase()
    {
        // Sets Up — add a rental to delete so we don't break other tests
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

        // calls the method
        await _repository.DeleteAsync(added.Id);
        var result = await _repository.GetByIdAsync(added.Id);

        // checks the result
        Assert.Null(result);
    }
}
