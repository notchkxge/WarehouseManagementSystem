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
    }
}