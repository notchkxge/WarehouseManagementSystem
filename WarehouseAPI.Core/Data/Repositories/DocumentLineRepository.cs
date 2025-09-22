using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Docs;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class DocumentLineRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentLineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentLine>> GetAllAsync()
        {
            return await _context.DocumentLines
                .Include(dl => dl.Document)
                .Include(dl => dl.Product)
                .ToListAsync();
        }

        public async Task<DocumentLine> GetByIdAsync(int id)
        {
            return await _context.DocumentLines
                .Include(dl => dl.Document)
                .Include(dl => dl.Product)
                .FirstOrDefaultAsync(dl => dl.Id == id);
        }

        public async Task<List<DocumentLine>> GetByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentLines
                .Where(dl => dl.DocumentId == documentId)
                .Include(dl => dl.Product)
                .ToListAsync();
        }

        public async Task<List<DocumentLine>> GetByProductIdAsync(int productId)
        {
            return await _context.DocumentLines
                .Where(dl => dl.ProductId == productId)
                .Include(dl => dl.Document)
                .ToListAsync();
        }

        public async Task<DocumentLine> CreateAsync(DocumentLine documentLine)
        {
            _context.DocumentLines.Add(documentLine);
            await _context.SaveChangesAsync();
            return documentLine;
        }

        public async Task<DocumentLine> UpdateAsync(DocumentLine documentLine)
        {
            _context.DocumentLines.Update(documentLine);
            await _context.SaveChangesAsync();
            return documentLine;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var documentLine = await _context.DocumentLines.FindAsync(id);
            if (documentLine == null) return false;
            
            _context.DocumentLines.Remove(documentLine);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}