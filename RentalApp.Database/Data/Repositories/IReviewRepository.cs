using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByItemAsync(int itemId);
    Task<double> GetAverageRatingAsync(int itemId);
}