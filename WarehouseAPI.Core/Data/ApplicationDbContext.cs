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
        //Отношения между документами
        modelBuilder.Entity<Document>(entity => {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Number).IsRequired().HasMaxLength(50);
            entity.Property(d => d.CreatedDate).IsRequired();

            entity.HasIndex(d => d.Number).IsUnique();

            //hasMany Documents for HasOne Author (многие к одному)
            entity.HasOne(d => d.Author)
                .WithMany(e => e.AuthoredDocuments)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            //hasOne Document for HasMany status (один ко многим)
            entity.HasOne(d => d.DocumentStatus)
                .WithMany(d => d.Documents)
                .HasForeignKey(d => d.DocumentStatusId);

            //hasOne Document for HasMany Type (один ко многим)
            entity.HasOne(d => d.DocumentType)
                .WithMany(d => d.Documents)
                .HasForeignKey(d => d.DocumentTypeId);
        });
        // Configure inheritance for document types
            modelBuilder.Entity<Document>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<GoodsReceipt>("GoodsReceipt")
                .HasValue<GoodsIssue>("GoodsIssue");

            modelBuilder.Entity<Employee>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Login).IsUnique();
            });

            modelBuilder.Entity<DocumentLine>(entity => {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Quantity).IsRequired();

                entity.HasOne(d => d.Document)
                    .WithMany(d => d.DocumentLines)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DocumentLines)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //add relations for warehouse/role and product
            modelBuilder.Entity<Warehouse>(entity => {
                
            });











    }
}