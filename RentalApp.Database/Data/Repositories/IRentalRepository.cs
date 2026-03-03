using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    Task<IEnumerable<Rental>> GetByBorrowerAsync(int borrowerId);
    Task<IEnumerable<Rental>> GetByItemOwnerAsync(int ownerId);
}