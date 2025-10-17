namespace WarehouseAPI.Core.Models.Docs;

public class GoodsReceiptIssue : Document{
    public DateTime DeliveryDate { get; set; }
}

public class GoodsIssue : Document{

    public DateTime SaleDate { get; set; }
}

