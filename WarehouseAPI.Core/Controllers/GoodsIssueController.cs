using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SimpleAuthorize("Storekeeper")]
    public class GoodsIssueController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GoodsIssueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/goodsissue - Create new issue in "новый" status
        [HttpPost]
        public async Task<ActionResult<GoodsIssue>> CreateGoodsIssue(CreateGoodsIssueDto dto)
        {
            var newStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "новый");
            var issueType = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Name == "GoodsIssue");

            if (newStatus == null || issueType == null)
                return BadRequest("Required document status or type not found");

            var issue = new GoodsIssue
            {
                Number = await GenerateDocumentNumber(),
                CreatedDate = DateTime.UtcNow,
                AuthorId = dto.AuthorId,
                DocumentStatusId = newStatus.Id,
                DocumentTypeId = issueType.Id,
                SaleDate = dto.SaleDate
            };

            _context.Documents.Add(issue); // Add to Documents DbSet
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGoodsIssue), new { id = issue.Id }, issue);
        }

        // POST: api/goodsissue/{id}/lines - Add product line
        [HttpPost("{id}/lines")]
        public async Task<ActionResult> AddProductLine(int id, AddProductLineDto dto)
        {
            var issue = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.DocumentLines)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (issue == null) return NotFound();

            // Check if product exists - ONLY from existing products
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return BadRequest("Product not found in catalog");

            // Check stock availability
            var totalStock = await _context.ProductBalances
                .Where(pb => pb.ProductId == dto.ProductId)
                .SumAsync(pb => pb.Quantity);

            if (totalStock < dto.Quantity)
                return BadRequest($"Not enough stock. Available: {totalStock}, Requested: {dto.Quantity}");

            // For Goods Issue: product can be added multiple times, increase quantity if exists
            var existingLine = issue.DocumentLines.FirstOrDefault(dl => dl.ProductId == dto.ProductId);
            if (existingLine != null)
            {
                // Increase quantity in existing line
                existingLine.Quantity += dto.Quantity;
            }
            else
            {
                var documentLine = new GoodsIssueLine
                {
                    DocumentId = id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                _context.DocumentLines.Add(documentLine);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/goodsissue/{id}/issue - Move to "выдано" status
        [HttpPut("{id}/issue")]
        public async Task<ActionResult> IssueGoods(int id)
        {
            var issue = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (issue == null) return NotFound();

            var issueStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "выдано");
            if (issueStatus == null) return BadRequest("Status not found");

            // Update status to "выдано"
            issue.DocumentStatusId = issueStatus.Id;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/goodsissue/{id}/close - Move to "закрыт", update inventory
        [HttpPut("{id}/close")]
        public async Task<ActionResult> CloseGoodsIssue(int id)
        {
            var issue = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (issue == null) return NotFound();

            var closeStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "закрыт");
            if (closeStatus == null) return BadRequest("Status not found");

            // Reduce inventory
            foreach (var line in issue.DocumentLines)
            {
                var productBalances = await _context.ProductBalances
                    .Where(pb => pb.ProductId == line.ProductId)
                    .OrderByDescending(pb => pb.Quantity)
                    .ToListAsync();

                var remainingQuantity = line.Quantity;

                foreach (var balance in productBalances)
                {
                    if (remainingQuantity <= 0) break;

                    if (balance.Quantity >= remainingQuantity)
                    {
                        balance.Quantity -= remainingQuantity;
                        remainingQuantity = 0;
                    }
                    else
                    {
                        remainingQuantity -= balance.Quantity;
                        balance.Quantity = 0;
                    }
                    balance.UpdateDate = DateTime.UtcNow;
                }

                if (remainingQuantity > 0)
                {
                    return BadRequest($"Not enough stock for product {line.Product.Name}. Missing: {remainingQuantity}");
                }
            }

            // Update document status to закрыт
            issue.DocumentStatusId = closeStatus.Id;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/goodsissue/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsIssue>> GetGoodsIssue(int id)
        {
            var issue = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentType)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (issue == null) return NotFound();
            return issue;
        }

        // GET: api/goodsissue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoodsIssue>>> GetGoodsIssues()
        {
            var issues = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .ToListAsync();

            return issues;
        }

        [HttpGet("report/csv")]
        public async Task<IActionResult> GetGoodsIssueReportCsv()
        {
            var issues = await _context.Documents.OfType<GoodsIssue>()
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Номер документа,Статус,Автор,Дата продажи,Товар,Количество,Всего товаров,Дата создания");

            foreach (var issue in issues)
            {
                var authorName = $"{issue.Author.FirstName} {issue.Author.LastName}";
                var totalItems = issue.DocumentLines.Sum(dl => dl.Quantity);

                foreach (var line in issue.DocumentLines)
                {
                    csv.AppendLine(
                        $"\"{issue.Number}\",\"{TranslateStatus(issue.DocumentStatus.Name)}\",\"{authorName}\",\"{issue.SaleDate:yyyy-MM-dd}\",\"{line.Product.Name}\",{line.Quantity},{totalItems},\"{issue.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }

                if (!issue.DocumentLines.Any())
                {
                    csv.AppendLine(
                        $"\"{issue.Number}\",\"{TranslateStatus(issue.DocumentStatus.Name)}\",\"{authorName}\",\"{issue.SaleDate:yyyy-MM-dd}\",\"Нет товаров\",0,0,\"{issue.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }
            }

            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);

            var fileName = $"Отчет_по_выдаче_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        private async Task<string> GenerateDocumentNumber()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.Documents.OfType<GoodsIssue>()
                .Where(gi => gi.CreatedDate.Date == DateTime.UtcNow.Date)
                .CountAsync();

            return $"GI-{today}-{count + 1:000}";
        }

        private string TranslateStatus(string status)
        {
            return status switch
            {
                "новый" => "Новый",
                "выдано" => "Выдано", 
                "закрыт" => "Закрыт",
                _ => status
            };
        }
    }
}