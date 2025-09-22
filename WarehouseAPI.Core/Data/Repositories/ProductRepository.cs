using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.DocumentLines)
                .Include(p => p.ProductBalances)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> SearchByNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<Product> GetByArticleNumberAsync(string articleNumber)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ArticleNumber == articleNumber);
        }

        public async Task<List<Product>> GetProductsInWarehouseAsync(int warehouseId)
        {
            return await _context.Products
                .Where(p => p.ProductBalances.Any(pb => pb.StorageLocation.WarehouseId == warehouseId))
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;
            
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}