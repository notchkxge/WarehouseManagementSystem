using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Core.Data.Repositories;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly WarehouseRepository _warehouseRepository;

        public WarehousesController(WarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        // GET: api/Warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return Ok(warehouses);
        }

        // GET: api/Warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            
            if (warehouse == null)
            {
                return NotFound();
            }

            return warehouse;
        }

        // GET: api/Warehouses/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Warehouse>> GetWarehouseByName(string name)
        {
            var warehouse = await _warehouseRepository.GetByNameAsync(name);
            
            if (warehouse == null)
            {
                return NotFound();
            }

            return warehouse;
        }

        // GET: api/Warehouses/5/full
        [HttpGet("{id}/full")]
        public async Task<ActionResult<Warehouse>> GetWarehouseWithAllData(int id)
        {
            var warehouse = await _warehouseRepository.GetWithAllDataAsync(id);
            
            if (warehouse == null)
            {
                return NotFound();
            }

            return warehouse;
        }

        // GET: api/Warehouses/5/capacity
        [HttpGet("{id}/capacity")]
        public async Task<ActionResult<object>> GetWarehouseCapacity(int id)
        {
            var warehouse = await _warehouseRepository.GetWithAllDataAsync(id);
            
            if (warehouse == null)
            {
                return NotFound();
            }

            var totalCapacity = warehouse.StorageLocations.Sum(sl => sl.Capacity);
            var usedCapacity = warehouse.StorageLocations.Sum(sl => sl.CurrentWeight);
            var availableCapacity = totalCapacity - usedCapacity;

            return new
            {
                TotalCapacity = totalCapacity,
                UsedCapacity = usedCapacity,
                AvailableCapacity = availableCapacity,
                UtilizationPercentage = totalCapacity > 0 ? (usedCapacity / totalCapacity) * 100 : 0,
                StorageLocationCount = warehouse.StorageLocations.Count
            };
        }

        // POST: api/Warehouses
        [HttpPost]
        public async Task<ActionResult<Warehouse>> PostWarehouse(Warehouse warehouse)
        {
            // Check if warehouse name already exists
            var existingWarehouse = await _warehouseRepository.GetByNameAsync(warehouse.Name);
            if (existingWarehouse != null)
            {
                return BadRequest("Warehouse with this name already exists.");
            }

            var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);
            
            return CreatedAtAction("GetWarehouse", new { id = createdWarehouse.Id }, createdWarehouse);
        }

        // PUT: api/Warehouses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarehouse(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
            {
                return BadRequest();
            }

            // Check if warehouse exists
            var existingWarehouse = await _warehouseRepository.GetByIdAsync(id);
            if (existingWarehouse == null)
            {
                return NotFound();
            }

            // Check if name is being changed to one that already exists
            if (warehouse.Name != existingWarehouse.Name)
            {
                var warehouseWithSameName = await _warehouseRepository.GetByNameAsync(warehouse.Name);
                if (warehouseWithSameName != null)
                {
                    return BadRequest("Warehouse with this name already exists.");
                }
            }

            await _warehouseRepository.UpdateAsync(warehouse);
            return NoContent();
        }

        // DELETE: api/Warehouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var result = await _warehouseRepository.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}