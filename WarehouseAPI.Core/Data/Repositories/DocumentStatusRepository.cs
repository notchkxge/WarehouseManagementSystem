using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories
{
    public class DocumentStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentStatus>> GetAllAsync()
        {
            return await _context.DocumentStatuses.ToListAsync();
        }

        public async Task<DocumentStatus> GetByIdAsync(int id)
        {
            return await _context.DocumentStatuses.FindAsync(id);
        }

        public async Task<DocumentStatus> GetByNameAsync(string name)
        {
            return await _context.DocumentStatuses
                .FirstOrDefaultAsync(ds => ds.Name == name);
        }

        public async Task<DocumentStatus> CreateAsync(DocumentStatus documentStatus)
        {
            _context.DocumentStatuses.Add(documentStatus);
            await _context.SaveChangesAsync();
            return documentStatus;
        }

        public async Task<DocumentStatus> UpdateAsync(DocumentStatus documentStatus)
        {
            _context.DocumentStatuses.Update(documentStatus);
            await _context.SaveChangesAsync();
            return documentStatus;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var documentStatus = await _context.DocumentStatuses.FindAsync(id);
            if (documentStatus == null) return false;
            
            _context.DocumentStatuses.Remove(documentStatus);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}