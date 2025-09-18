namespace WarehouseAPI.Core.Data.Repositories;

public class DocumentRepository{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context){
        _context = context;
    }

    public List<ProductFrequencyInfo> GetFrequentlyMovedProducts()
    {
        return _context.DocumentLines
            .Join(_context.Documents,
                line => line.DocumentId,
                doc => doc.Id,
                (line, doc) => new { line, doc })
            .Join(_context.DocumentTypes,
                combined => combined.doc.DocumentTypeId,
                type => type.Id,
                (combined, type) => new { combined.doc, combined.line, type })
            .Where(combined => combined.type.Name == "GoodsReceipt" || combined.type.Name == "GoodsIssue")
            .Join(_context.Products,
                temp => temp.line.ProductId,
                product => product.Id,
                (temp, product) => new { 
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ArticleNumber = product.ArticleNumber,
                })
            // Group by product to count frequency
            .GroupBy(x => new { x.ProductId, x.ProductName, x.ArticleNumber })
            .Select(g => new ProductFrequencyInfo
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                ArticleNumber = g.Key.ArticleNumber,
                FrequencyCount = g.Count() // Count how many times this product appears
            })
            .OrderByDescending(x => x.FrequencyCount) // Order by frequency
            .ToList();
    }

    public class ProductFrequencyInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ArticleNumber { get; set; }
        public int FrequencyCount { get; set; } 
    }
}