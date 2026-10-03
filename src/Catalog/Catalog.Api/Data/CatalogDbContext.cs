using Catalog.Api.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("Products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Sku).HasMaxLength(32).IsRequired();
            product.HasIndex(p => p.Sku).IsUnique();
            product.Property(p => p.Name).HasMaxLength(200).IsRequired();
            product.Property(p => p.Price).HasPrecision(10, 2);
            product.Property(p => p.Currency).HasMaxLength(3).IsFixedLength();

            product.HasData(CatalogSeedData.Products);
        });
    }
}
