using Catalog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Products;

internal static class ProductEndpoints
{
    // Protects the database from huge IN (...) lists
    private const int MaxIdsPerRequest = 100;

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/v1/products").WithTags("Products");

        products.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("List products, optionally only the given ids (?ids=a&ids=b)");

        products.MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById");

        return app;
    }

    private static async Task<IResult> GetProductsAsync(
        [FromQuery] Guid[]? ids,
        CatalogDbContext db,
        CancellationToken cancellationToken)
    {
        if (ids is { Length: > MaxIdsPerRequest })
        {
            return TypedResults.Problem(
                detail: $"At most {MaxIdsPerRequest} ids can be requested at once.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var query = db.Products.AsNoTracking();

        if (ids is { Length: > 0 })
        {
            query = query.Where(p => ids.Contains(p.Id));
        }

        var result = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.Price, p.Currency))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        CatalogDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ProductDto.From(product));
    }
}
