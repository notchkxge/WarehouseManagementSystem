using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Models.Docs;

public class InventoryDocumentLine
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int ProductId { get; set; }
    public int StorageLocationId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime RecordedDate { get; set; }

    // Navigation properties
    public InventoryDocument Document { get; set; }
    public Product Product { get; set; }
    public StorageLocation StorageLocation { get; set; }
}