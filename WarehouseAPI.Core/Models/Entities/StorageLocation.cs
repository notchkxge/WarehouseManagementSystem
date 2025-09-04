using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.Entities;

public class StorageLocation{
    public int Id { get; set; }
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
    public const double SpotVolume = 1.0;
    public int WarehouseId{ get; set; }
    public virtual Warehouse Warehouse { get; set; }
    public bool CanAddWeight(double additionalWeight)
    {
        var rackSpots = Warehouse.StorageLocations
            .Where(s => s.Building == Building && 
                        s.Room == Room && 
                        s.Rack == Rack);
        double totalRackWeight = rackSpots.Sum(s => s.CurrentWeight);
            
        return (totalRackWeight + additionalWeight) <= 300;
    }
    public override string ToString()
    {
        return $"{Building}-{Room}-{Rack}-{Spot}";
    }
}