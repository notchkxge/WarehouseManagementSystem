using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class DocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentLines)
                .ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .Include(d => d.DocumentType)
                .Include(d => d.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Document>> GetByTypeIdAsync(int typeId)
        {
            return await _context.Documents
                .Where(d => d.DocumentTypeId == typeId)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByStatusIdAsync(int statusId)
        {
            return await _context.Documents
                .Where(d => d.DocumentStatusId == statusId)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByAuthorIdAsync(int authorId)
        {
            return await _context.Documents
                .Where(d => d.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<Document> CreateAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<Document> UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;
            
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}