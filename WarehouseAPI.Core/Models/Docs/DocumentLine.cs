using WarehouseAPI.Core.Models.Entities;
namespace WarehouseAPI.Core.Models.Docs;

public abstract class DocumentLine{
    public int Id { get; set; }
    public decimal Quantity { get; set; }
    
    public int DocumentId { get; set; }
    public int ProductId { get; set; }
    
    public virtual Document Document { get; set; }
    public virtual Product Product { get; set; }
}