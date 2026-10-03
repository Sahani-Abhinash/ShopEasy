using System.Net;
using System.Net.Http.Json;
using Catalog.Api.Data;
using Catalog.Api.Products;

namespace Catalog.Api.IntegrationTests;

[Collection(CatalogApiTestGroup.Name)]
public sealed class ProductEndpointsTests(CatalogApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProductsReturnsAllSeededProductsSortedByName()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>(
            "/api/v1/products", TestContext.Current.CancellationToken);

        Assert.NotNull(products);
        Assert.Equal(["27\" 4K Monitor", "Mechanical Keyboard", "Wireless Mouse"], products.Select(p => p.Name));
    }

    [Fact]
    public async Task GetProductsWithIdsReturnsOnlyRequestedProducts()
    {
        var url = $"/api/v1/products?ids={CatalogSeedData.KeyboardId}&ids={CatalogSeedData.MonitorId}";

        var products = await _client.GetFromJsonAsync<List<ProductDto>>(url, TestContext.Current.CancellationToken);

        Assert.NotNull(products);
        Assert.Equal(
            new[] { CatalogSeedData.KeyboardId, CatalogSeedData.MonitorId }.Order(),
            products.Select(p => p.Id).Order());
    }

    [Fact]
    public async Task GetProductsWithTooManyIdsReturnsBadRequest()
    {
        var ids = string.Join("&", Enumerable.Range(0, 101).Select(_ => $"ids={Guid.NewGuid()}"));

        var response = await _client.GetAsync($"/api/v1/products?{ids}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetProductByIdReturnsProduct()
    {
        var product = await _client.GetFromJsonAsync<ProductDto>(
            $"/api/v1/products/{CatalogSeedData.KeyboardId}", TestContext.Current.CancellationToken);

        Assert.NotNull(product);
        Assert.Equal("KB-001", product.Sku);
        Assert.Equal(89.99m, product.Price);
        Assert.Equal("EUR", product.Currency);
    }

    [Fact]
    public async Task GetUnknownProductReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
