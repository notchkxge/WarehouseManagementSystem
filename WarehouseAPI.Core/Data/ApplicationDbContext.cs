using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Core.Models.Docs;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Data;

public class ApplicationDbContext : DbContext{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
//entity
    public DbSet<Employee> Employees{ get; set;} = null!;
    public DbSet<Warehouse> Warehouses{ get; set; } = null!;
    public DbSet<Role> Roles{ get; set; } = null!;
    public DbSet<StorageLocation> Locations{ get; set; } = null!;
    public DbSet<Product> Products{ get; set; } = null!;

    
//Documents
    public DbSet<Document> Documents{ get; set; } = null!;
    public DbSet<DocumentStatus> DocumentStatuses{ get; set; } = null!;
    public DbSet<DocumentType> DocumentsTypes{ get; set; } = null!;
    public DbSet<DocumentLine> DocumentLines{ get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        
        
        
        
        
        
        
        
    }
}