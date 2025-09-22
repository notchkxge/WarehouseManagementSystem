using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class ProductBalanceRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductBalanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductBalance>> GetAllAsync()
        {
            return await _context.ProductBalances
                .Include(pb => pb.Product)
                .Include(pb => pb.StorageLocation)
                .ToListAsync();
        }

        public async Task<ProductBalance> GetByIdAsync(int id)
        {
            return await _context.ProductBalances
                .Include(pb => pb.Product)
                .Include(pb => pb.StorageLocation)
                .FirstOrDefaultAsync(pb => pb.Id == id);
        }

        public async Task<List<ProductBalance>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductBalances
                .Where(pb => pb.ProductId == productId)
                .Include(pb => pb.StorageLocation)
                .ToListAsync();
        }

        public async Task<List<ProductBalance>> GetByLocationIdAsync(int locationId)
        {
            return await _context.ProductBalances
                .Where(pb => pb.StorageLocationId == locationId)
                .Include(pb => pb.Product)
                .ToListAsync();
        }

        public async Task<ProductBalance> CreateAsync(ProductBalance productBalance)
        {
            _context.ProductBalances.Add(productBalance);
            await _context.SaveChangesAsync();
            return productBalance;
        }

        public async Task<ProductBalance> UpdateAsync(ProductBalance productBalance)
        {
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

        public async Task<bool> AdjustQuantityAsync(int productId, int locationId, int quantityChange)
        {
            var productBalance = await _context.ProductBalances
                .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.StorageLocationId == locationId);
            
            if (productBalance == null) return false;
            
            productBalance.Quantity += quantityChange;
            if (productBalance.Quantity < 0) productBalance.Quantity = 0;
            
            _context.ProductBalances.Update(productBalance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}