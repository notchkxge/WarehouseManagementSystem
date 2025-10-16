using System.Text;
using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Core.Data.Repositories;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ProductRepository _productRepository; 
        private readonly StorageLocationRepository _storageLocationRepository; 

        public InventoryController(
            InventoryRepository inventoryRepository,
            ProductRepository productRepository,
            StorageLocationRepository storageLocationRepository) 
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository; 
            _storageLocationRepository = storageLocationRepository; 
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductBalance>>> GetInventory()
        {
            var inventory = await _inventoryRepository.GetAllAsync();
            return Ok(inventory);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductBalance>> GetInventoryItem(int id)
        {
            var item = await _inventoryRepository.GetByIdAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductBalance>>> GetLowStock([FromQuery] decimal threshold = 10)
        {
            var lowStock = await _inventoryRepository.GetLowStockAsync(threshold);
            return Ok(lowStock);
        }

        // FIXED: Only one POST method, removed duplicate
        [HttpPost]
        public async Task<ActionResult<ProductBalance>> PostInventoryItem(CreateInventoryDto createDto)
        {
            // Check if product exists
            var product = await _productRepository.GetByIdAsync(createDto.ProductId);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            // Check if storage location exists
            var storageLocation = await _storageLocationRepository.GetByIdAsync(createDto.StorageLocationId);
            if (storageLocation == null)
            {
                return BadRequest("Storage location not found.");
            }

            // Check if inventory item already exists for this product and location
            var existingItem = await _inventoryRepository.GetByProductAndLocationAsync(createDto.ProductId, createDto.StorageLocationId);
            if (existingItem != null)
            {
                return BadRequest("Inventory item for this product and location already exists.");
            }

            var inventoryItem = new ProductBalance
            {
                ProductId = createDto.ProductId,
                StorageLocationId = createDto.StorageLocationId,
                Quantity = createDto.Quantity,
                UpdateDate = DateTime.UtcNow
            };

            var createdItem = await _inventoryRepository.CreateAsync(inventoryItem);
            return CreatedAtAction("GetInventoryItem", new { id = createdItem.Id }, createdItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventoryItem(int id, ProductBalance inventoryItem)
        {
            if (id != inventoryItem.Id) return BadRequest();
            await _inventoryRepository.UpdateAsync(inventoryItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var result = await _inventoryRepository.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpGet("report/csv")]
        public async Task<IActionResult> GetInventoryReportCsv()
        {
            var inventory = await _inventoryRepository.GetAllAsync();
    
            // Russian headers
            var csv = new StringBuilder();
            csv.AppendLine("Наименование товара,Артикул,Цена,Вес,Склад,Место хранения,Количество,Последнее обновление");
    
            foreach (var item in inventory)
            {
                var location = $"{item.StorageLocation.Building}-{item.StorageLocation.Room}-{item.StorageLocation.Rack}-{item.StorageLocation.Spot}";
                csv.AppendLine($"\"{item.Product.Name}\",\"{item.Product.ArticleNumber}\",{item.Product.Price},{item.Product.Weight},\"{item.StorageLocation.Warehouse.Name}\",\"{location}\",{item.Quantity},\"{item.UpdateDate:yyyy-MM-dd HH:mm}\"");
            }
    
            // Save to folder
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath))
            {
                Directory.CreateDirectory(reportsPath);
            }
    
            var fileName = $"Отчет_по_остаткам_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
    
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }
        [HttpGet("reports/list")]
        public IActionResult GetSavedReports()
        {
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
    
            if (!Directory.Exists(reportsPath))
            {
                return Ok(new { message = "No reports generated yet" });
            }
    
            var files = Directory.GetFiles(reportsPath, "*.csv")
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    FilePath = f,
                    CreatedDate = System.IO.File.GetCreationTime(f),
                    Size = new FileInfo(f).Length
                })
                .OrderByDescending(f => f.CreatedDate)
                .ToList();
    
            return Ok(files);
        }
        [HttpGet("report/status/csv")]
        public async Task<IActionResult> GetInventoryStatusReportCsv()
        {
            var inventory = await _inventoryRepository.GetAllAsync();
    
            // Russian headers
            var csv = new StringBuilder();
            csv.AppendLine("Товар,Артикул,Цена,Место хранения,Количество,Статус запаса,Последнее обновление");
    
            foreach (var item in inventory)
            {
                var location = $"{item.StorageLocation.Building}-{item.StorageLocation.Room}-{item.StorageLocation.Rack}-{item.StorageLocation.Spot}";
                var stockStatus = item.Quantity == 0 ? "НЕТ В НАЛИЧИИ" : 
                    item.Quantity <= 10 ? "НИЗКИЙ ЗАПАС" : "В НАЛИЧИИ";
        
                csv.AppendLine($"\"{item.Product.Name}\",\"{item.Product.ArticleNumber}\",{item.Product.Price},\"{location}\",{item.Quantity},\"{stockStatus}\",\"{item.UpdateDate:yyyy-MM-dd HH:mm}\"");
            }
    
            // Save to folder
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);
    
            var fileName = $"Статус_запасов_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
    
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }
    }
}