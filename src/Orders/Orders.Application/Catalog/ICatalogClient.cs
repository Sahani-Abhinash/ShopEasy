namespace Orders.Application.Catalog;

/// <summary>
/// Port to the Catalog service. The handler doesn't know whether this is HTTP, gRPC, or a cache.
/// </summary>
public interface ICatalogClient
{
    /// <summary>Returns the requested products that exist. Unknown ids are simply missing.</summary>
    /// <exception cref="CatalogUnavailableException">Catalog can't be reached or failed.</exception>
    Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
}

public sealed record CatalogProduct(Guid Id, string Name, decimal Price, string Currency);

public sealed class CatalogUnavailableException : Exception
{
    public CatalogUnavailableException()
    {
    }

    public CatalogUnavailableException(string message)
        : base(message)
    {
    }

    public CatalogUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
