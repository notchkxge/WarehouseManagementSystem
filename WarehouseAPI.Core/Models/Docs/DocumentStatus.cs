using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.Docs;

public class DocumentStatus{
    public int Id{ get; set; }
    [MaxLength(50)]
    public string Name{ get; set; } = null!;

    public virtual ICollection<Document> Documents{ get; set; } = new List<Document>();
}