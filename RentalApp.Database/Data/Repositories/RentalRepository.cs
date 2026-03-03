using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Rental>> GetAllAsync()
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Rental?> GetByIdAsync(int id)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rental> AddAsync(Rental entity)
    {
        _context.Rentals.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Rental> UpdateAsync(Rental entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Rentals.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental != null)
        {
            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Rental>> GetByBorrowerAsync(int borrowerId)
    {
        return await _context.Rentals
            .Where(r => r.BorrowerId == borrowerId)
            .Include(r => r.Item)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rental>> GetByItemOwnerAsync(int ownerId)
    {
        return await _context.Rentals
            .Include(r => r.Item)
            .Include(r => r.Borrower)
            .Where(r => r.Item!.OwnerId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}