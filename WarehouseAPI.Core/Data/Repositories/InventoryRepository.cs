using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class InventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductBalance>> GetAllAsync()
        {
            return await _context.ProductBalances
                .Include(pb => pb.Product)
                .Include(pb => pb.StorageLocation)
                .ThenInclude(sl => sl.Warehouse)
                .ToListAsync();
        }

        public async Task<ProductBalance> GetByIdAsync(int id)
        {
            return await _context.ProductBalances
                .Include(pb => pb.Product)
                .Include(pb => pb.StorageLocation)
                .ThenInclude(sl => sl.Warehouse)
                .FirstOrDefaultAsync(pb => pb.Id == id);
        }

        public async Task<List<ProductBalance>> GetLowStockAsync(decimal threshold = 10)
        {
            return await _context.ProductBalances
                .Include(pb => pb.Product)
                .Include(pb => pb.StorageLocation)
                .ThenInclude(sl => sl.Warehouse)
                .Where(pb => pb.Quantity <= threshold)
                .ToListAsync();
        }

        // ADD THIS MISSING METHOD:
        public async Task<ProductBalance> GetByProductAndLocationAsync(int productId, int storageLocationId)
        {
            return await _context.ProductBalances
                .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.StorageLocationId == storageLocationId);
        }

        public async Task<ProductBalance> CreateAsync(ProductBalance productBalance)
        {
            productBalance.UpdateDate = DateTime.UtcNow;
            _context.ProductBalances.Add(productBalance);
            await _context.SaveChangesAsync();
            return productBalance;
        }

        public async Task<ProductBalance> UpdateAsync(ProductBalance productBalance)
        {
            productBalance.UpdateDate = DateTime.UtcNow;
            _context.ProductBalances.Update(productBalance);
            await _context.SaveChangesAsync();
            return productBalance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var productBalance = await _context.ProductBalances.FindAsync(id);
            if (productBalance == null) return false;
            
            _context.ProductBalances.Remove(productBalance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}