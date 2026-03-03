using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IItemRepository : IRepository<Item>
{
    Task<IEnumerable<Item>> GetByOwnerAsync(int ownerId);
    Task<IEnumerable<Item>> GetByCategoryAsync(int categoryId);
}