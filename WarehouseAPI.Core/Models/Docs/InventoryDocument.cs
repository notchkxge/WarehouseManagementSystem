namespace WarehouseAPI.Core.Models.Docs;

public class InventoryDocument : Document
{
    public ICollection<InventoryDocumentLine> DocumentLines { get; set; }
}