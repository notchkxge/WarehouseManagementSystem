using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Models.Docs;

public abstract class Document{
    public int Id{ get; set; }
    [Required]
    [MaxLength(50)]
    public string Number{ get; set; } = null!;
    public DateTime CreatedDate{ get; set; } = DateTime.UtcNow;
    
    //foreign key
    public int AuthorId{ get; set; }
    public int DocumentStatusId{ get; set; }
    public int DocumentTypeId{ get; set; }
    
    //navigation many ot one
    public virtual Employee Author{ get; set; }
    public virtual DocumentStatus DocumentStatus{ get; set; }
    public virtual DocumentType DocumentType  { get; set; }

    public virtual ICollection<DocumentLine> DocumentLines{ get; set; } = new List<DocumentLine>();


}