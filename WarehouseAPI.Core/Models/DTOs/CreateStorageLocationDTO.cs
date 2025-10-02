using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.DTOs
{
    public class CreateStorageLocationDto
    {
        [Required]
        [MaxLength(100)]
        public string Building { get; set; }

        [Required]
        [MaxLength(100)]
        public string Room { get; set; }

        [Required]
        [MaxLength(100)]
        public string Rack { get; set; }

        [Required]
        [MaxLength(100)]
        public string Spot { get; set; }

        public double CurrentWeight { get; set; }

        public int Capacity { get; set; }

        [Required]
        public int WarehouseId { get; set; }
    }
}