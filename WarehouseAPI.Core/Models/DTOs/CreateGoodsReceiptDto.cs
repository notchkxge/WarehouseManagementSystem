namespace WarehouseAPI.Core.Models.Docs;

public class CreateGoodsReceiptDto
{
    public int AuthorId { get; set; }
    public DateTime DeliveryDate { get; set; }
}

public class AddProductLineDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}

public class AssignLocationsDto
{
    public List<AssignmentDto> Assignments { get; set; } = new();
}

public class AssignmentDto
{
    public int DocumentLineId { get; set; }
    public int StorageLocationId { get; set; }
}