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
    public DbSet<ProductBalance> ProductBalances { get; set; } = null!;

    
//Documents
    public DbSet<Document> Documents{ get; set; } = null!;
    public DbSet<DocumentStatus> DocumentStatuses{ get; set; } = null!;
    public DbSet<DocumentType> DocumentTypes{ get; set; } = null!;
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

            //add relations for warehouse/role /product and storageLocation
            modelBuilder.Entity<Role>(entity => {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.Name);

                entity.HasMany(r => r.Employees)
                    .WithOne(e => e.Role)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Warehouse>(entity => {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(w => w.Name);
                entity.Property(w => w.Address).IsRequired();

                entity.HasMany(w => w.StorageLocations)
                    .WithOne(s => s.Warehouse)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //A Product can be used in multiple DocumentLines
            modelBuilder.Entity<Product>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Name);
                entity.Property(p=> p.ArticleNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(p => p.ArticleNumber).IsUnique();
                entity.Property(p => p.Price).IsRequired();
                entity.Property(p=> p.Weight).IsRequired();
                entity.Property(p => p.Dimension).IsRequired();

                entity.HasMany(p => p.DocumentLines)
                    .WithOne(dl => dl.Product)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StorageLocation>(entity => {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Building).IsRequired().HasMaxLength(100);
                entity.Property(s=> s.Room).IsRequired().HasMaxLength(100);
                entity.Property(s=> s.Rack).IsRequired().HasMaxLength(100);
                entity.Property(s=> s.Spot).IsRequired().HasMaxLength(100);
                entity.Property(s => s.CurrentWeight);
                entity.HasIndex(s => new{ s.Building, s.Room, s.Rack, s.Spot }).IsUnique();
                
                entity.HasOne(s => s.Warehouse)
                    .WithMany(w => w.StorageLocations)
                    .HasForeignKey(s => s.WarehouseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //add DocumentStatus and DocumentType

            modelBuilder.Entity<DocumentStatus>(entity => {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(50);

                entity.HasMany(d => d.Documents)
                    .WithOne(d => d.DocumentStatus)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DocumentType>(entity => {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(50);

                entity.HasMany(d => d.Documents)
                    .WithOne(d => d.DocumentType)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            //add ProductBalance
            modelBuilder.Entity<ProductBalance>(entity => {
                entity.HasKey(pb => pb.Id);
    
                entity.HasOne(pb => pb.Product)
                    .WithMany(p => p.ProductBalances)
                    .HasForeignKey(pb => pb.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
        
                entity.HasOne(pb => pb.StorageLocation)
                    .WithMany(sl => sl.ProductBalances)
                    .HasForeignKey(pb => pb.StorageLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
    }
}