using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SimpleAuthorize("Director", "Storekeeper")]
    public class InventoryDocumentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateInventoryDocument(CreateInventoryDocumentDto dto)
        {
            try
            {
                // Ensure required status and type exist
                await EnsureDocumentStatusAndTypeExist();

                var newStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "новый");
                var inventoryType = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Name == "InventoryDocument");

                var document = new InventoryDocument
                {
                    Number = await GenerateDocumentNumber(),
                    CreatedDate = DateTime.UtcNow,
                    AuthorId = dto.AuthorId,
                    DocumentStatusId = newStatus.Id,
                    DocumentTypeId = inventoryType.Id
                };

                _context.InventoryDocuments.Add(document);
                await _context.SaveChangesAsync();

                // Create inventory snapshot lines
                var inventoryItems = await _context.ProductBalances
                    .Include(pb => pb.Product)
                    .Include(pb => pb.StorageLocation)
                    .ToListAsync();

                foreach (var item in inventoryItems)
                {
                    var line = new InventoryDocumentLine
                    {
                        DocumentId = document.Id,
                        ProductId = item.ProductId,
                        StorageLocationId = item.StorageLocationId,
                        Quantity = item.Quantity,
                        RecordedDate = DateTime.UtcNow
                    };
                    _context.InventoryDocumentLines.Add(line);
                }

                await _context.SaveChangesAsync();

                // Return the created document with lines
                var createdDocument = await _context.InventoryDocuments
                    .Include(d => d.Author)
                    .Include(d => d.DocumentStatus)
                    .Include(d => d.DocumentType)
                    .FirstOrDefaultAsync(d => d.Id == document.Id);

                var documentLines = await _context.InventoryDocumentLines
                    .Where(dl => dl.DocumentId == document.Id)
                    .Include(dl => dl.Product)
                    .Include(dl => dl.StorageLocation)
                    .ToListAsync();

                var result = new
                {
                    createdDocument.Id,
                    createdDocument.Number,
                    createdDocument.CreatedDate,
                    createdDocument.AuthorId,
                    createdDocument.DocumentStatusId,
                    createdDocument.DocumentTypeId,
                    Author = createdDocument.Author,
                    DocumentStatus = createdDocument.DocumentStatus,
                    DocumentType = createdDocument.DocumentType,
                    DocumentLines = documentLines
                };

                return CreatedAtAction(nameof(GetInventoryDocument), new { id = document.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error creating inventory document: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetInventoryDocument(int id)
        {
            var document = await _context.InventoryDocuments
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .Include(d => d.DocumentType)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null) return NotFound();

            // Load document lines separately to avoid inheritance issues
            var documentLines = await _context.InventoryDocumentLines
                .Where(dl => dl.DocumentId == id)
                .Include(dl => dl.Product)
                .Include(dl => dl.StorageLocation)
                .ToListAsync();

            // Create response object
            var result = new
            {
                document.Id,
                document.Number,
                document.CreatedDate,
                document.AuthorId,
                document.DocumentStatusId,
                document.DocumentTypeId,
                Author = document.Author,
                DocumentStatus = document.DocumentStatus,
                DocumentType = document.DocumentType,
                DocumentLines = documentLines
            };

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInventoryDocuments()
        {
            var documents = await _context.InventoryDocuments
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .ToListAsync();

            // Get all document lines in one query
            var documentIds = documents.Select(d => d.Id).ToList();
            var allDocumentLines = await _context.InventoryDocumentLines
                .Where(dl => documentIds.Contains(dl.DocumentId))
                .Include(dl => dl.Product)
                .Include(dl => dl.StorageLocation)
                .ToListAsync();

            // Group document lines by document ID
            var documentLinesByDocId = allDocumentLines
                .GroupBy(dl => dl.DocumentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Create response objects
            var result = documents.Select(document => new
            {
                document.Id,
                document.Number,
                document.CreatedDate,
                document.AuthorId,
                document.DocumentStatusId,
                document.DocumentTypeId,
                Author = document.Author,
                DocumentStatus = document.DocumentStatus,
                DocumentType = document.DocumentType,
                DocumentLines = documentLinesByDocId.GetValueOrDefault(document.Id, new List<InventoryDocumentLine>())
            });

            return Ok(result);
        }

        [HttpGet("report/csv")]
        public async Task<IActionResult> GetInventoryDocumentReportCsv()
        {
            var documents = await _context.InventoryDocuments
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .ToListAsync();

            // Get all document lines with related data
            var documentIds = documents.Select(d => d.Id).ToList();
            var allDocumentLines = await _context.InventoryDocumentLines
                .Where(dl => documentIds.Contains(dl.DocumentId))
                .Include(dl => dl.Product)
                .Include(dl => dl.StorageLocation)
                .ToListAsync();

            var documentLinesByDocId = allDocumentLines
                .GroupBy(dl => dl.DocumentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var csv = new StringBuilder();
            csv.AppendLine("Номер документа,Автор,Дата создания,Статус,Товар,Место хранения,Количество,Дата записи");

            foreach (var document in documents)
            {
                var authorName = $"{document.Author.FirstName} {document.Author.LastName}";
                var documentLines = documentLinesByDocId.GetValueOrDefault(document.Id, new List<InventoryDocumentLine>());

                foreach (var line in documentLines)
                {
                    var location = line.StorageLocation != null 
                        ? $"{line.StorageLocation.Building}-{line.StorageLocation.Room}-{line.StorageLocation.Rack}-{line.StorageLocation.Spot}"
                        : "Не указано";
                    
                    csv.AppendLine($"\"{document.Number}\",\"{authorName}\",\"{document.CreatedDate:yyyy-MM-dd HH:mm}\",\"{document.DocumentStatus.Name}\",\"{line.Product?.Name ?? "Неизвестный товар"}\",\"{location}\",{line.Quantity},\"{line.RecordedDate:yyyy-MM-dd HH:mm}\"");
                }

                if (!documentLines.Any())
                {
                    csv.AppendLine($"\"{document.Number}\",\"{authorName}\",\"{document.CreatedDate:yyyy-MM-dd HH:mm}\",\"{document.DocumentStatus.Name}\",\"Нет данных\",\"-\",0,\"-\"");
                }
            }

            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);

            var fileName = $"Инвентаризация_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        [HttpGet("debug/status")]
        public async Task<ActionResult> DebugStatus()
        {
            var statuses = await _context.DocumentStatuses.ToListAsync();
            var types = await _context.DocumentTypes.ToListAsync();
            
            return Ok(new {
                Statuses = statuses,
                DocumentTypes = types
            });
        }

        private async Task EnsureDocumentStatusAndTypeExist()
        {
            // Ensure "новый" status exists
            if (!await _context.DocumentStatuses.AnyAsync(s => s.Name == "новый"))
            {
                _context.DocumentStatuses.Add(new DocumentStatus { Name = "новый" });
            }

            // Ensure "InventoryDocument" type exists
            if (!await _context.DocumentTypes.AnyAsync(t => t.Name == "InventoryDocument"))
            {
                _context.DocumentTypes.Add(new DocumentType { Name = "InventoryDocument" });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> GenerateDocumentNumber()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.InventoryDocuments
                .Where(d => d.CreatedDate.Date == DateTime.UtcNow.Date)
                .CountAsync();

            return $"INV-{today}-{count + 1:000}";
        }
    }
}