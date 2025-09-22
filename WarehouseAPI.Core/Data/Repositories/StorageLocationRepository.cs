using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class StorageLocationRepository
    {
        private readonly ApplicationDbContext _context;

        public StorageLocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StorageLocation>> GetAllAsync()
        {
            return await _context.Locations
                .Include(sl => sl.Warehouse)
                .Include(sl => sl.ProductBalances)
                .ToListAsync();
        }

        public async Task<StorageLocation> GetByIdAsync(int id)
        {
            return await _context.Locations
                .Include(sl => sl.Warehouse)
                .Include(sl => sl.ProductBalances)
                .FirstOrDefaultAsync(sl => sl.Id == id);
        }

        public async Task<List<StorageLocation>> GetByWarehouseIdAsync(int warehouseId)
        {
            return await _context.Locations
                .Where(sl => sl.WarehouseId == warehouseId)
                .ToListAsync();
        }
        public async Task<StorageLocation> CreateAsync(StorageLocation location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<StorageLocation> UpdateAsync(StorageLocation location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return false;
            
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StorageLocation>> FindAvailableLocationsAsync(int warehouseId)
        {
            return await _context.Locations
                .Where(sl => sl.WarehouseId == warehouseId)
                .ToListAsync();
        }
    }
}