using System.ComponentModel.DataAnnotations;
using WarehouseAPI.Core.Models.Docs;

namespace WarehouseAPI.Core.Models.Entities;

public class Employee{ 
    public int Id{ get; set;}
    [Required]
    [MaxLength(100)]
    public string? FirstName{ get; set;}
    
    [Required]
    [MaxLength(100)]
    public string? LastName{ get; set;}

    public bool IsActive{ get; set;} = true;

    [Required]
    [MaxLength(100)]
    public string? Login{ get; set;}
    [Required]
    public string? PasswordHash{ get; set;}

    public int RoleId{ get; set; }
    
    public virtual Role Role{ get; set;}

    //many to one
    public virtual ICollection<Document> AuthoredDocuments { get; set; } = new List<Document>();
}

