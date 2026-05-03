using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
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
        // keeps the PostGIS point in sync with the lat/lng fields
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        entity.Location = factory.CreatePoint(new Coordinate(entity.Longitude, entity.Latitude));

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

    public async Task<IEnumerable<Item>> GetNearbyItemsAsync(
        double latitude, double longitude, double radiusMiles)
    {
        var radiusMetres = radiusMiles * 1609.344;
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var userLocation = factory.CreatePoint(new Coordinate(longitude, latitude));

        // ST_DWithin via EF Core spatial uses the geography (point) column index
        return await _context.Items
            .Where(i => i.IsAvailable
                     && i.Location != null
                     && i.Location.IsWithinDistance(userLocation, radiusMetres))
            .Include(i => i.Owner)
            .OrderBy(i => i.Location!.Distance(userLocation))
            .ToListAsync();
    }

}