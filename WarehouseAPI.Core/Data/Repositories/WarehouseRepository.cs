using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class WarehouseRepository
    {
        private readonly ApplicationDbContext _context;

        public WarehouseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _context.Warehouses.ToListAsync();
        }

        public async Task<Warehouse> GetByIdAsync(int id)
        {
            return await _context.Warehouses
                .Include(w => w.StorageLocations) 
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Warehouse> GetByNameAsync(string name)
        {
            return await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Name == name);
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null) return false;
            
            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            return true;
        }

        // Additional useful method: Get warehouse with all related data
        public async Task<Warehouse> GetWithAllDataAsync(int id)
        {
            return await _context.Warehouses
                .Include(w => w.StorageLocations)
                    .ThenInclude(sl => sl.ProductBalances)
                        .ThenInclude(pb => pb.Product)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}