using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.DTOs;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SimpleAuthorize("Director", "Storekeeper")]
    public class StorageReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StorageReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateStorageReport(CreateStorageReportDto dto)
        {
            try
            {
                // Ensure required status and type exist
                await EnsureDocumentStatusAndTypeExist();

                var newStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "новый");
                var storageReportType = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Name == "StorageReport");

                var document = new StorageReportDocument
                {
                    Number = await GenerateDocumentNumber(),
                    CreatedDate = DateTime.UtcNow,
                    AuthorId = dto.AuthorId,
                    DocumentStatusId = newStatus.Id,
                    DocumentTypeId = storageReportType.Id
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Create storage location report lines
                var storageLocations = await _context.Locations
                    .Include(sl => sl.Warehouse)
                    .Include(sl => sl.ProductBalances)
                        .ThenInclude(pb => pb.Product)
                    .ToListAsync();

                foreach (var location in storageLocations)
                {
                    var totalWeight = location.ProductBalances?
                        .Sum(pb => (double)(pb.Product?.Weight ?? 0) * (double)pb.Quantity) ?? 0;
    
                    var volumePercentage = (totalWeight / 300.0) * 100; // 300kg max capacity per rack
    
                    var line = new StorageReportDocumentLine
                    {
                        DocumentId = document.Id,
                        StorageLocationId = location.Id,
                        CurrentWeight = totalWeight,
                        VolumePercentage = Math.Round((decimal)volumePercentage, 2),
                        RecordedDate = DateTime.UtcNow
                    };
                    _context.StorageReportDocumentLines.Add(line);
                }

                await _context.SaveChangesAsync();

                // Return the created document with lines
                var createdDocument = await _context.StorageReportDocuments
                    .Include(d => d.Author)
                    .Include(d => d.DocumentStatus)
                    .Include(d => d.DocumentType)
                    .FirstOrDefaultAsync(d => d.Id == document.Id);

                var documentLines = await _context.StorageReportDocumentLines
                    .Where(dl => dl.DocumentId == document.Id)
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

                return CreatedAtAction(nameof(GetStorageReport), new { id = document.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error creating storage report: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetStorageReport(int id)
        {
            var document = await _context.Documents.OfType<StorageReportDocument>()
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .Include(d => d.DocumentType)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null) return NotFound();

            // Load document lines separately
            var documentLines = await _context.StorageReportDocumentLines
                .Where(dl => dl.DocumentId == id)
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
        public async Task<ActionResult<IEnumerable<object>>> GetStorageReports()
        {
            var documents = await _context.Documents.OfType<StorageReportDocument>()
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .ToListAsync();

            // Get all document lines in one query
            var documentIds = documents.Select(d => d.Id).ToList();
            var allDocumentLines = await _context.StorageReportDocumentLines
                .Where(dl => documentIds.Contains(dl.DocumentId))
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
                DocumentLines = documentLinesByDocId.GetValueOrDefault(document.Id, new List<StorageReportDocumentLine>())
            });

            return Ok(result);
        }

        [HttpGet("report/csv")]
        public async Task<IActionResult> GetStorageReportCsv()
        {
            var documents = await _context.StorageReportDocuments
                .Include(d => d.Author)
                .Include(d => d.DocumentStatus)
                .ToListAsync();

            // Get all document lines with related data
            var documentIds = documents.Select(d => d.Id).ToList();
            var allDocumentLines = await _context.StorageReportDocumentLines
                .Where(dl => documentIds.Contains(dl.DocumentId))
                .Include(dl => dl.StorageLocation)
                .ToListAsync();

            var documentLinesByDocId = allDocumentLines
                .GroupBy(dl => dl.DocumentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var csv = new StringBuilder();
            csv.AppendLine("Номер документа,Автор,Дата создания,Статус,Место хранения,Текущий вес,Максимальный вес,Использование %,Статус загрузки");

            foreach (var document in documents)
            {
                var authorName = $"{document.Author.FirstName} {document.Author.LastName}";
                var documentLines = documentLinesByDocId.GetValueOrDefault(document.Id, new List<StorageReportDocumentLine>());

                foreach (var line in documentLines)
                {
                    var location = line.StorageLocation != null 
                        ? $"{line.StorageLocation.Building}-{line.StorageLocation.Room}-{line.StorageLocation.Rack}-{line.StorageLocation.Spot}"
                        : "Не указано";
                    
                    var loadStatus = line.VolumePercentage >= 90 ? "ПЕРЕГРУЗКА" : 
                                   line.VolumePercentage >= 70 ? "ВЫСОКАЯ" : 
                                   "НОРМА";

                    csv.AppendLine($"\"{document.Number}\",\"{authorName}\",\"{document.CreatedDate:yyyy-MM-dd HH:mm}\",\"{document.DocumentStatus.Name}\",\"{location}\",{line.CurrentWeight},300,{line.VolumePercentage}%,\"{loadStatus}\"");
                }

                if (!documentLines.Any())
                {
                    csv.AppendLine($"\"{document.Number}\",\"{authorName}\",\"{document.CreatedDate:yyyy-MM-dd HH:mm}\",\"{document.DocumentStatus.Name}\",\"Нет данных\",0,300,0%,\"НОРМА\"");
                }
            }

            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);

            var fileName = $"Отчет_по_местам_хранения_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
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

            // Ensure "StorageReport" type exists
            if (!await _context.DocumentTypes.AnyAsync(t => t.Name == "StorageReport"))
            {
                _context.DocumentTypes.Add(new DocumentType { Name = "StorageReport" });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> GenerateDocumentNumber()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Documents.OfType<StorageReportDocument>()
                .Where(d => d.CreatedDate.Date == DateTime.UtcNow.Date)
                .CountAsync();

            return $"SR-{today}-{count + 1:000}";
        }
    }
}