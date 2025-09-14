using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Core.Models.Docs;

namespace WarehouseAPI.Core.Models.Entities;

public class Product{
    public int Id{ get; set; }
    [Required]
    [MaxLength(100)]
    public string Name{ get; set; } = null!;

    [Required] 
    [MaxLength(50)]
    public string ArticleNumber{ get; set; } = null!;
    [Required]
    public double Price{ get; set; }
    [Required]
    public double Weight{ get; set; }
    [Required]
    public double Dimension{ get; set; }
    [Required]
    public int Quantity{ get; set; }

    public virtual ICollection<DocumentLine> DocumentLines{ get; set; } = new List<DocumentLine>();
    public virtual ICollection<ProductBalance> ProductBalances{ get; set; } = new List<ProductBalance>();
}  