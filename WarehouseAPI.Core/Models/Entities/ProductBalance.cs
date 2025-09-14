namespace WarehouseAPI.Core.Models.Entities;

public class ProductBalance{
    public int Id{ get; set; }
    public int ProductId{ get; set; }
    public int StorageLocationId { get; set; }
    public decimal Quantity{ get; set; }
    public DateTime UpdateDate{ get; set; } = DateTime.UtcNow;
    
    public Product Product { get; set; }
    public StorageLocation StorageLocation { get; set; }
}