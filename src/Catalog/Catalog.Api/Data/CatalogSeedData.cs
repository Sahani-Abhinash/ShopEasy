using Catalog.Api.Products;

namespace Catalog.Api.Data;

/// <summary>
/// Seed products, part of the migration so every environment starts identically.
/// Ids are fixed so other services and tests can rely on them.
/// </summary>
internal static class CatalogSeedData
{
    public static readonly Guid KeyboardId = Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1001");
    public static readonly Guid MouseId = Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1002");
    public static readonly Guid MonitorId = Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1003");

    public static Product[] Products =>
    [
        new() { Id = KeyboardId, Sku = "KB-001", Name = "Mechanical Keyboard", Price = 89.99m, Currency = "EUR" },
        new() { Id = MouseId, Sku = "MS-001", Name = "Wireless Mouse", Price = 29.99m, Currency = "EUR" },
        new() { Id = MonitorId, Sku = "MN-001", Name = "27\" 4K Monitor", Price = 349.00m, Currency = "EUR" },
    ];
}
