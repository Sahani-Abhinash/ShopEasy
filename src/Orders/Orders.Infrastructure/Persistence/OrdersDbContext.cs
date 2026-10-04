using Microsoft.EntityFrameworkCore;
using Orders.Domain;

namespace Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    internal const string IdempotencyKeyProperty = "IdempotencyKey";

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("Orders");
            order.HasKey(o => o.Id);
            order.Property(o => o.CustomerId).IsRequired();
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(32);
            order.Property(o => o.Currency).HasMaxLength(3).IsFixedLength();
            order.Property(o => o.CreatedAt);
            order.Ignore(o => o.Total);

            // Idempotency key is persistence detail, not part of the domain model (shadow property).
            // The unique index is the real guarantee against duplicate orders from retries.
            order.Property<string>(IdempotencyKeyProperty).HasMaxLength(100).IsRequired();
            order.HasIndex(nameof(Order.CustomerId), IdempotencyKeyProperty).IsUnique();

            // Optimistic concurrency via PostgreSQL's xmin system column
            order.Property<uint>("Version").IsRowVersion();

            order.OwnsMany(o => o.Items, item =>
            {
                item.ToTable("OrderItems");
                item.WithOwner().HasForeignKey("OrderId");
                item.Property<int>("Id");
                item.HasKey("Id");
                item.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
                item.Property(i => i.UnitPrice).HasPrecision(10, 2);
                item.Ignore(i => i.LineTotal);
            });
            order.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
