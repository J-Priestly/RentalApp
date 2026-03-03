using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _context.Items
            .Where(i => i.IsAvailable)
            .Include(i => i.Owner)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Reviews)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Item> AddAsync(Item entity)
    {
        _context.Items.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Item> UpdateAsync(Item entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Items.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            item.IsAvailable = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Item>> GetByOwnerAsync(int ownerId)
    {
        return await _context.Items
            .Where(i => i.OwnerId == ownerId && i.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Items
            .Where(i => i.CategoryId == categoryId && i.IsAvailable)
            .ToListAsync();
    }
}