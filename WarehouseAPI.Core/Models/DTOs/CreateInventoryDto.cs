using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.DTOs
{
    public class CreateInventoryDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int StorageLocationId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}