using WarehouseAPI.Core.Models.Entities;
namespace WarehouseAPI.Core.Models.Docs;

public abstract class DocumentLine{//removed abstarct to do teh damn migration DB
    public int Id { get; set; }
    public decimal Quantity { get; set; }
    
    public int DocumentId { get; set; }
    public int ProductId { get; set; }
    
    public virtual Document Document { get; set; }
    public virtual Product Product { get; set; }
}
// Concrete implementations
public class GoodsReceiptLine : DocumentLine
{
    // Additional properties specific to goods receipt lines
    public string BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class GoodsIssueLine : DocumentLine
{
    // Additional properties specific to goods issue lines
    public string PickedBy { get; set; }
}