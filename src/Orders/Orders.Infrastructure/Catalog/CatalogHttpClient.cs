using System.Net.Http.Json;
using Orders.Application.Catalog;

namespace Orders.Infrastructure.Catalog;

/// <summary>
/// Calls Catalog's batch endpoint: one request for all products of an order.
/// Retries, timeouts, and the circuit breaker are added by the resilience handler (see DependencyInjection).
/// </summary>
internal sealed class CatalogHttpClient(HttpClient http) : ICatalogClient
{
    public async Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(productIds);

        if (productIds.Count == 0)
        {
            return [];
        }

        var query = string.Join("&", productIds.Select(id => $"ids={id}"));

        try
        {
            var products = await http.GetFromJsonAsync<List<CatalogProductResponse>>(
                new Uri($"api/v1/products?{query}", UriKind.Relative), cancellationToken);

            return products?.Select(p => new CatalogProduct(p.Id, p.Name, p.Price, p.Currency)).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            // Network errors, timeouts, open circuit, 5xx, invalid JSON: Catalog is unavailable for this request
            throw new CatalogUnavailableException("Catalog service is unavailable.", ex);
        }
    }

    // Our own copy of Catalog's contract: services never share model classes
    private sealed record CatalogProductResponse(Guid Id, string Sku, string Name, decimal Price, string Currency);
}
