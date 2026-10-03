namespace Catalog.Api.Products;

/// <summary>
/// Public API contract. Kept separate from the <see cref="Product"/> entity
/// so table changes don't change the API (Chapter 2, §5).
/// </summary>
internal sealed record ProductDto(Guid Id, string Sku, string Name, decimal Price, string Currency)
{
    public static ProductDto From(Product product) =>
        new(product.Id, product.Sku, product.Name, product.Price, product.Currency);
}
