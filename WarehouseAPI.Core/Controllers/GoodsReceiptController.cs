using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Data;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers{
    [ApiController]
    [Route("api/[controller]")]
    [SimpleAuthorize("Storekeeper")]
    public class GoodsReceiptController : ControllerBase{
        private readonly ApplicationDbContext _context;

        public GoodsReceiptController(ApplicationDbContext context){
            _context = context;
        }

        // POST: api/goodsreceipt - Create new receipt in "новый" status
        [HttpPost]
        public async Task<ActionResult<GoodsReceiptIssue>> CreateGoodsReceipt(CreateGoodsReceiptDto dto){
            // Get status "новый"
            var newStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "новый");
            var receiptType = await _context.DocumentTypes.FirstOrDefaultAsync(t => t.Name == "GoodsReceipt");

            if (newStatus == null || receiptType == null)
                return BadRequest("Required document status or type not found");

            var receipt = new GoodsReceiptIssue{
                Number = await GenerateDocumentNumber(),
                CreatedDate = DateTime.UtcNow,
                AuthorId = dto.AuthorId,
                DocumentStatusId = newStatus.Id,
                DocumentTypeId = receiptType.Id,
                DeliveryDate = dto.DeliveryDate
            };

            _context.GoodsReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGoodsReceipt), new{ id = receipt.Id }, receipt);
        }

        // POST: api/goodsreceipt/{id}/lines - Add product line (unique per document)
        [HttpPost("{id}/lines")]
        public async Task<ActionResult> AddProductLine(int id, AddProductLineDto dto){
            var receipt = await _context.GoodsReceipts
                .Include(r => r.DocumentLines)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return NotFound();

            // Check if product already exists in this document
            if (receipt.DocumentLines.Any(dl => dl.ProductId == dto.ProductId))
                return BadRequest("Product already exists in this document");

            // Check if product exists
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return BadRequest("Product not found");

            // FIXED: Use GoodsReceiptLine instead of abstract DocumentLine
            var documentLine = new GoodsReceiptLine{
                DocumentId = id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            _context.DocumentLines.Add(documentLine);
            await _context.SaveChangesAsync();

            return Ok(new{ documentLineId = documentLine.Id });
        }

        // PUT: api/goodsreceipt/{id}/assign-locations - Move to "раскладка", assign locations
        [HttpPut("{id}/assign-locations")]
        public async Task<ActionResult> AssignStorageLocations(int id, AssignLocationsDto dto)
        {
            var receipt = await _context.GoodsReceipts
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return NotFound();

            // Get status "раскладка"
            var assignStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "раскладка");
            if (assignStatus == null) return BadRequest("Status not found");

            // Validate capacity for each assignment AND CREATE THEM
            foreach (var assignment in dto.Assignments)
            {
                var documentLine = receipt.DocumentLines.FirstOrDefault(dl => dl.Id == assignment.DocumentLineId);
                var storageLocation = await _context.Locations
                    .Include(sl => sl.Warehouse)
                    .ThenInclude(w => w.StorageLocations)
                    .FirstOrDefaultAsync(sl => sl.Id == assignment.StorageLocationId);

                if (documentLine == null || storageLocation == null)
                    return BadRequest("Invalid document line or storage location");

                // Check capacity
                if (!await CanAddProductToLocation(storageLocation, documentLine.Product, documentLine.Quantity))
                    return BadRequest($"Not enough capacity in location {storageLocation}");

                // ✅ FIX: CREATE THE ASSIGNMENT IN DATABASE
                var goodsReceiptAssignment = new GoodsReceiptLineAssignment
                {
                    DocumentLineId = assignment.DocumentLineId,
                    StorageLocationId = assignment.StorageLocationId,
                };
                _context.GoodsReceiptLineAssignments.Add(goodsReceiptAssignment);
            }

            // Update status to "раскладка"
            receipt.DocumentStatusId = assignStatus.Id;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/goodsreceipt/{id}/close - Move to "закрыт", update inventory
        [HttpPut("{id}/close")]
public async Task<ActionResult> CloseGoodsReceipt(int id)
{
    var receipt = await _context.GoodsReceipts
        .Include(r => r.DocumentLines)
        .ThenInclude(dl => dl.Product)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (receipt == null) return NotFound();

    // Get status "закрыт"
    var closeStatus = await _context.DocumentStatuses.FirstOrDefaultAsync(s => s.Name == "закрыт");
    if (closeStatus == null) return BadRequest("Status not found");

    // Update inventory AND storage location weights
    foreach (var line in receipt.DocumentLines)
    {
        // Find the assignment for this line
        var assignment = await _context.GoodsReceiptLineAssignments
            .FirstOrDefaultAsync(a => a.DocumentLineId == line.Id);
        
        if (assignment != null)
        {
            var storageLocation = await _context.Locations
                .FirstOrDefaultAsync(sl => sl.Id == assignment.StorageLocationId);
            
            if (storageLocation != null)
            {
                // CALCULATE AND UPDATE WEIGHT - FIXED
                double productWeight = (double)line.Product.Weight;
                double additionalWeight = productWeight * (double)line.Quantity;
                storageLocation.CurrentWeight += additionalWeight;

                // Update or create product balance
                var productBalance = await _context.ProductBalances
                    .FirstOrDefaultAsync(pb => pb.ProductId == line.ProductId && 
                                              pb.StorageLocationId == storageLocation.Id);

                if (productBalance != null)
                {
                    productBalance.Quantity += line.Quantity;
                    productBalance.UpdateDate = DateTime.UtcNow;
                }
                else
                {
                    productBalance = new ProductBalance
                    {
                        ProductId = line.ProductId,
                        StorageLocationId = storageLocation.Id,
                        Quantity = line.Quantity,
                        UpdateDate = DateTime.UtcNow
                    };
                    _context.ProductBalances.Add(productBalance);
                }
            }
        }
    }

    // Update document status
    receipt.DocumentStatusId = closeStatus.Id;
    await _context.SaveChangesAsync();

    return Ok();
}

        // GET: api/goodsreceipt/{id} - Get receipt details
        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsReceiptIssue>> GetGoodsReceipt(int id){
            var receipt = await _context.GoodsReceipts
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentType)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return NotFound();
            return receipt;
        }

        // GET: api/goodsreceipt - Get all receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoodsReceiptIssue>>> GetGoodsReceipts(){
            var receipts = await _context.GoodsReceipts
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .ToListAsync();

            return receipts;
        }

        private async Task<string> GenerateDocumentNumber(){
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.GoodsReceipts
                .Where(gr => gr.CreatedDate.Date == DateTime.UtcNow.Date)
                .CountAsync();

            return $"GR-{today}-{count + 1:000}";
        }

        private async Task<bool> CanAddProductToLocation(StorageLocation location, Product product, decimal quantity){
            // Check rack weight capacity (300kg per rack)
            var rackSpots = await _context.Locations
                .Where(sl => sl.Building == location.Building &&
                             sl.Room == location.Room &&
                             sl.Rack == location.Rack)
                .ToListAsync();

            double totalRackWeight = rackSpots.Sum(sl => sl.CurrentWeight);
            double additionalWeight = product.Weight * (double)quantity;

            return (totalRackWeight + additionalWeight) <= 300.0;
        }

        [HttpGet("report/csv")]
        public async Task<IActionResult> GetGoodsReceiptReportCsv(){
            var receipts = await _context.GoodsReceipts
                .Include(r => r.Author)
                .Include(r => r.DocumentStatus)
                .Include(r => r.DocumentLines)
                .ThenInclude(dl => dl.Product)
                .ToListAsync();

            // Russian headers
            var csv = new StringBuilder();
            csv.AppendLine("Номер документа,Статус,Автор,Дата поставки,Товар,Количество,Всего товаров,Дата создания");

            foreach (var receipt in receipts){
                var authorName = $"{receipt.Author.FirstName} {receipt.Author.LastName}";
                var totalItems = receipt.DocumentLines.Sum(dl => dl.Quantity);

                foreach (var line in receipt.DocumentLines){
                    csv.AppendLine(
                        $"\"{receipt.Number}\",\"{TranslateStatus(receipt.DocumentStatus.Name)}\",\"{authorName}\",\"{receipt.DeliveryDate:yyyy-MM-dd}\",\"{line.Product.Name}\",{line.Quantity},{totalItems},\"{receipt.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }

                if (!receipt.DocumentLines.Any()){
                    csv.AppendLine(
                        $"\"{receipt.Number}\",\"{TranslateStatus(receipt.DocumentStatus.Name)}\",\"{authorName}\",\"{receipt.DeliveryDate:yyyy-MM-dd}\",\"Нет товаров\",0,0,\"{receipt.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }
            }

            // Save to folder
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)){
                Directory.CreateDirectory(reportsPath);
            }

            var fileName = $"Отчет_по_приемкам_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

// Helper method to translate status names
        private string TranslateStatus(string status){
            return status switch{
                "новый" => "Новый",
                "раскладка" => "Раскладка",
                "закрыт" => "Закрыт",
                "выдано" => "Выдано",
                _ => status
            };
        }
        [HttpGet("report/workflow/csv")]
public async Task<IActionResult> GetGoodsReceiptWorkflowReportCsv()
{
    var receipts = await _context.GoodsReceipts
        .Include(r => r.Author)
        .Include(r => r.DocumentStatus)
        .Include(r => r.DocumentLines)
        .ThenInclude(dl => dl.Product)
        .ToListAsync();
    
    // Russian headers
    var csv = new StringBuilder();
    csv.AppendLine("Номер,Статус,Автор,Дата создания,Дата поставки,Товары,Общее количество,Статус документа");
    
    foreach (var receipt in receipts)
    {
        var authorName = $"{receipt.Author.FirstName} {receipt.Author.LastName}";
        var products = string.Join("; ", receipt.DocumentLines.Select(dl => $"{dl.Product.Name} ({dl.Quantity})"));
        var totalQuantity = receipt.DocumentLines.Sum(dl => dl.Quantity);
        var statusDescription = GetStatusDescription(receipt.DocumentStatus.Name);
        
        csv.AppendLine($"\"{receipt.Number}\",\"{receipt.DocumentStatus.Name}\",\"{authorName}\",\"{receipt.CreatedDate:yyyy-MM-dd HH:mm}\",\"{receipt.DeliveryDate:yyyy-MM-dd}\",\"{products}\",{totalQuantity},\"{statusDescription}\"");
    }
    
    // Save to folder
    var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
    if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);
    
    var fileName = $"Рабочий_процесс_приемки_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    var filePath = Path.Combine(reportsPath, fileName);
    await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
    
    var bytes = Encoding.UTF8.GetBytes(csv.ToString());
    return File(bytes, "text/csv", fileName);
}

private string GetStatusDescription(string status)
{
    return status switch
    {
        "новый" => "Документ создан, ожидает добавления товаров",
        "раскладка" => "Товары добавлены, ожидает назначения мест хранения",
        "закрыт" => "Документ завершен, товары добавлены на склад",
        "выдано" => "Товары выданы со склада",
        _ => "Неизвестный статус"
    };
}
    }
}