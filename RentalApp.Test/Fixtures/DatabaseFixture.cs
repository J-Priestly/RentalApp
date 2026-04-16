using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Test.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        Seed();
    }

    public void Seed()
    {
        Context.Reviews.RemoveRange(Context.Reviews);
        Context.Rentals.RemoveRange(Context.Rentals);
        Context.Items.RemoveRange(Context.Items);
        Context.Users.RemoveRange(Context.Users);
        Context.SaveChanges();

        Context.Users.AddRange(
            new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Owner",
                Email = "owner@test.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FirstName = "Test",
                LastName = "Borrower",
                Email = "borrower@test.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        Context.Items.Add(new Item
        {
            Id = 1,
            Title = "Test Drill",
            Description = "A test power drill",
            DailyRate = 10.00m,
            CategoryId = 1,
            OwnerId = 1,
            IsAvailable = true,
            Latitude = 55.9533,
            Longitude = -3.1883,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        Context.Rentals.Add(new Rental
        {
            Id = 1,
            ItemId = 1,
            BorrowerId = 2,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(3),
            Status = RentalStatus.Requested,
            TotalPrice = 20.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
