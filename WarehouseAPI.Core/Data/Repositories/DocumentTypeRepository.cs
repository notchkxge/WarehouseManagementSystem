using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class DocumentTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentType>> GetAllAsync()
        {
            return await _context.DocumentTypes.ToListAsync();
        }

        public async Task<DocumentType> GetByIdAsync(int id)
        {
            return await _context.DocumentTypes.FindAsync(id);
        }

        public async Task<DocumentType> GetByNameAsync(string name)
        {
            return await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.Name == name);
        }

        public async Task<DocumentType> CreateAsync(DocumentType documentType)
        {
            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();
            return documentType;
        }

        public async Task<DocumentType> UpdateAsync(DocumentType documentType)
        {
            _context.DocumentTypes.Update(documentType);
            await _context.SaveChangesAsync();
            return documentType;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType == null) return false;
            
            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}