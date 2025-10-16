using System.Text;
using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Core.Data.Repositories;
using WarehouseAPI.Core.Models.DTOs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageLocationsController : ControllerBase
    {
        private readonly StorageLocationRepository _storageLocationRepository;
        private readonly WarehouseRepository _warehouseRepository;

        public StorageLocationsController(StorageLocationRepository storageLocationRepository, WarehouseRepository warehouseRepository)
        {
            _storageLocationRepository = storageLocationRepository;
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StorageLocation>>> GetStorageLocations()
        {
            var locations = await _storageLocationRepository.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageLocation>> GetStorageLocation(int id)
        {
            var location = await _storageLocationRepository.GetByIdAsync(id);
            if (location == null) return NotFound();
            return location;
        }

        // FIXED: Only one POST method, using WarehouseRepository instead of _context
        [HttpPost]
        public async Task<ActionResult<StorageLocation>> PostStorageLocation(CreateStorageLocationDto createDto)
        {
            // Check if warehouse exists using WarehouseRepository
            var warehouse = await _warehouseRepository.GetByIdAsync(createDto.WarehouseId);
            if (warehouse == null)
            {
                return BadRequest("Warehouse not found.");
            }

            var storageLocation = new StorageLocation
            {
                Building = createDto.Building,
                Room = createDto.Room,
                Rack = createDto.Rack,
                Spot = createDto.Spot,
                CurrentWeight = createDto.CurrentWeight,
                Capacity = createDto.Capacity,
                WarehouseId = createDto.WarehouseId
            };

            var createdLocation = await _storageLocationRepository.CreateAsync(storageLocation);
            return CreatedAtAction("GetStorageLocation", new { id = createdLocation.Id }, createdLocation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStorageLocation(int id, StorageLocation location)
        {
            if (id != location.Id) return BadRequest();
            
            // Check if warehouse exists
            var warehouse = await _warehouseRepository.GetByIdAsync(location.WarehouseId);
            if (warehouse == null)
            {
                return BadRequest("Warehouse not found.");
            }

            await _storageLocationRepository.UpdateAsync(location);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStorageLocation(int id)
        {
            var result = await _storageLocationRepository.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpGet("report/capacity/csv")]
        public async Task<IActionResult> GetStorageCapacityReportCsv()
        {
            var locations = await _storageLocationRepository.GetAllAsync();

            // Russian headers
            var csv = new StringBuilder();
            csv.AppendLine("Склад,Место хранения,Текущий вес,Максимальная нагрузка,Использование %,Товары,Статус загрузки");

            foreach (var location in locations)
            {
                var fullLocation = $"{location.Building}-{location.Room}-{location.Rack}-{location.Spot}";
                var utilization = location.CurrentWeight / 300.0 * 100; // 300kg per rack
        
                // NULL-SAFE: Handle null ProductBalances and null Products
                var products = location.ProductBalances?
                    .Where(pb => pb != null) // Filter out null ProductBalance objects
                    .Select(pb => $"{pb.Product?.Name ?? "Unknown Product"} ({pb.Quantity})") ?? Enumerable.Empty<string>();
        
                var productsString = string.Join("; ", products);
                var loadStatus = utilization >= 90 ? "ПЕРЕГРУЗКА" : 
                    utilization >= 70 ? "ВЫСОКАЯ" : 
                    "НОРМА";

                csv.AppendLine($"\"{location.Warehouse?.Name ?? "Unknown Warehouse"}\",\"{fullLocation}\",{location.CurrentWeight},300,{utilization:F1}%,\"{productsString}\",\"{loadStatus}\"");
            }

            // Save to folder
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Generated");
            if (!Directory.Exists(reportsPath)) Directory.CreateDirectory(reportsPath);

            var fileName = $"Загрузка_склада_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(reportsPath, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }
    }
}