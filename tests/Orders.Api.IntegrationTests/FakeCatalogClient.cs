using Orders.Application.Catalog;

namespace Orders.Api.IntegrationTests;

/// <summary>
/// Replaces the HTTP Catalog client in tests: Orders is tested on its own, without running Catalog.
/// (Contract tests between Orders and Catalog come in Sprint 11.)
/// </summary>
public sealed class FakeCatalogClient : ICatalogClient
{
    public static readonly Guid KeyboardId = Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1001");
    public static readonly Guid MonitorId = Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1003");

    private static readonly CatalogProduct[] Products =
    [
        new(KeyboardId, "Mechanical Keyboard", 89.99m, "EUR"),
        new(MonitorId, "27\" 4K Monitor", 349.00m, "EUR"),
    ];

    /// <summary>When true, behaves like Catalog being down.</summary>
    public bool IsUnavailable { get; set; }

    public Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        if (IsUnavailable)
        {
            throw new CatalogUnavailableException("Catalog is down (fake).");
        }

        IReadOnlyList<CatalogProduct> result = [.. Products.Where(p => productIds.Contains(p.Id))];
        return Task.FromResult(result);
    }
}
