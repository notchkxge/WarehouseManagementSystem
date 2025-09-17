using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data.Repositories;

public class ProductRepository{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context){
        _context = context;
    }

    public List<ProductLocationInfo> GetProductsWithLocations(){
        return _context.Products
            .Join(_context.ProductBalances,
                product => product.Id,
                balance => balance.ProductId,
                (product, balance) => new{ product, balance })
            .Join(_context.Locations,
                combined => combined.balance.StorageLocationId,
                location => location.Id,
                (combined, location) => new {combined.product,combined.balance,location})
            .Join(_context.Warehouses,
                temp => temp.location.WarehouseId,
                warehouse => warehouse.Id,
                (temp, warehouse) => new ProductLocationInfo{
                    ProductName = temp.product.Name,
                    ArticleNumber = temp.product.ArticleNumber,
                    Quantity = temp.balance.Quantity,
                    Location = $"{temp.location.Building}-{temp.location.Room}-{temp.location.Rack}-{temp.location.Spot}",
                    WarehouseName = warehouse.Name
                })
            .ToList();
    }
}

public class ProductLocationInfo{
    public string ProductName{ get; set; }
    public string ArticleNumber{ get; set; }
    public decimal Quantity{ get; set; }
    public string Location{ get; set; }
    public string WarehouseName{ get; set; }
}