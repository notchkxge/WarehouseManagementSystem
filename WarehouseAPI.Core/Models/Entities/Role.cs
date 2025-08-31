using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Core.Models.Entities;

public class Role{
    public int Id{ get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set;}

    public virtual ICollection<Employee> Employees{ get; set; } //to link it with Employees class
}