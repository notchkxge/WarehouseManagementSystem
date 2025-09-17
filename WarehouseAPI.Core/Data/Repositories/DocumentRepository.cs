namespace WarehouseAPI.Core.Data.Repositories;

public class DocumentRepository{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context){
        _context = context;
    }

    public List<ProductFrequencyInfo> GetFrequentlyMovedProducts(){
        return _context.DocumentLines //after context should be a conenctor table
            .Join(_context.Documents,
                docId => docId.DocumentId,
                doc => doc.Id,
                (docId, doc) => new{ docId, doc })

            .Join(_context.DocumentTypes,
                combined => combined.doc.DocumentTypeId,
                type => type.Id,
                (combined, type) => new{ combined.doc, combined.docId, type })
            .Join(_context.Products,
                temp => temp.docId.ProductId,
                product => product.Id,
                (temp, product) => new ProductFrequencyInfo{
                    ProductId = temp.docId.ProductId,
                    ProductName = temp.type.Name,
                    Date = temp.doc.CreatedDate,
                    
                    
                })
            
            
            
            .ToList();






    }

    public class ProductFrequencyInfo{
        
    }
    
}