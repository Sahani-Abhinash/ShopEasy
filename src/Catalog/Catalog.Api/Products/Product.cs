namespace Catalog.Api.Products;

internal sealed class Product
{
    public Guid Id { get; init; }

    public required string Sku { get; init; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; init; } = "EUR";
}
