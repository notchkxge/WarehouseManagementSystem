using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.Entities;

public class Warehouse{
    public int Id{ get; set; }
    [Required] [MaxLength(100)] public string Name{ get; set; } = string.Empty;
    [Required]
    public int Capacity{ get; set; }

    public virtual ICollection<StorageLocation> StorageLocations{ get; set; } = new List<StorageLocation>();
}