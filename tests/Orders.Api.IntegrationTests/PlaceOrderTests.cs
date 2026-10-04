using System.Net;
using System.Net.Http.Json;
using Orders.Api.Orders;

namespace Orders.Api.IntegrationTests;

[Collection(OrdersApiTestGroup.Name)]
public sealed class PlaceOrderTests(OrdersApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient ClientFor(Guid customerId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Customer-Id", customerId.ToString());
        return client;
    }

    private static HttpRequestMessage PlaceOrderRequest(string? idempotencyKey, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders") { Content = JsonContent.Create(body) };
        if (idempotencyKey is not null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return request;
    }

    private static object TwoKeyboardsAndMonitor => new
    {
        items = new object[]
        {
            new { productId = FakeCatalogClient.KeyboardId, quantity = 2, price = 0.01m }, // price must be ignored
            new { productId = FakeCatalogClient.MonitorId, quantity = 1 },
        },
    };

    [Fact]
    public async Task PlaceOrderReturnsAcceptedAndStoresCatalogPrices()
    {
        using var client = ClientFor(Guid.NewGuid());

        using var response = await client.SendAsync(PlaceOrderRequest($"key-{Guid.NewGuid()}", TwoKeyboardsAndMonitor), Ct);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var placed = await response.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        Assert.NotNull(placed);
        Assert.Equal("Pending", placed.Status);
        Assert.Equal($"/api/v1/orders/{placed.OrderId}", response.Headers.Location?.ToString());

        var order = await client.GetFromJsonAsync<OrderResponse>(response.Headers.Location, Ct);
        Assert.NotNull(order);
        Assert.Equal(89.99m, order.Items.Single(i => i.ProductId == FakeCatalogClient.KeyboardId).UnitPrice);
        Assert.Equal("Mechanical Keyboard", order.Items.Single(i => i.ProductId == FakeCatalogClient.KeyboardId).ProductName);
        Assert.Equal(528.98m, order.Total);
        Assert.Equal("EUR", order.Currency);
    }

    [Fact]
    public async Task SameKeyTwiceReturnsSameOrderAndStoresOneRow()
    {
        var customerId = Guid.NewGuid();
        var key = $"key-{Guid.NewGuid()}";
        using var client = ClientFor(customerId);

        using var first = await client.SendAsync(PlaceOrderRequest(key, TwoKeyboardsAndMonitor), Ct);
        using var second = await client.SendAsync(PlaceOrderRequest(key, TwoKeyboardsAndMonitor), Ct);

        var firstOrder = await first.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        var secondOrder = await second.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        Assert.Equal(HttpStatusCode.Accepted, second.StatusCode);
        Assert.Equal(firstOrder?.OrderId, secondOrder?.OrderId);
        Assert.Equal(1, await factory.CountOrdersWithKeyAsync(customerId, key));
    }

    [Fact]
    public async Task ParallelRequestsWithSameKeyCreateOneOrder()
    {
        var customerId = Guid.NewGuid();
        var key = $"key-{Guid.NewGuid()}";
        using var client = ClientFor(customerId);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(async _ =>
        {
            using var response = await client.SendAsync(PlaceOrderRequest(key, TwoKeyboardsAndMonitor), Ct);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            return await response.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        }));

        Assert.Single(responses.Select(r => r?.OrderId).Distinct());
        Assert.Equal(1, await factory.CountOrdersWithKeyAsync(customerId, key));
    }

    [Fact]
    public async Task SameKeyFromDifferentCustomersCreatesTwoOrders()
    {
        var key = $"key-{Guid.NewGuid()}";
        using var alice = ClientFor(Guid.NewGuid());
        using var bob = ClientFor(Guid.NewGuid());

        using var aliceResponse = await alice.SendAsync(PlaceOrderRequest(key, TwoKeyboardsAndMonitor), Ct);
        using var bobResponse = await bob.SendAsync(PlaceOrderRequest(key, TwoKeyboardsAndMonitor), Ct);

        var aliceOrder = await aliceResponse.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        var bobOrder = await bobResponse.Content.ReadFromJsonAsync<PlaceOrderResponse>(Ct);
        Assert.NotEqual(aliceOrder?.OrderId, bobOrder?.OrderId);
    }

    [Fact]
    public async Task MissingIdempotencyKeyReturnsBadRequest()
    {
        using var client = ClientFor(Guid.NewGuid());

        using var response = await client.SendAsync(PlaceOrderRequest(null, TwoKeyboardsAndMonitor), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownProductReturnsUnprocessableEntity()
    {
        using var client = ClientFor(Guid.NewGuid());
        var body = new { items = new[] { new { productId = Guid.NewGuid(), quantity = 1 } } };

        using var response = await client.SendAsync(PlaceOrderRequest($"key-{Guid.NewGuid()}", body), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task ZeroQuantityReturnsBadRequest()
    {
        using var client = ClientFor(Guid.NewGuid());
        var body = new { items = new[] { new { productId = FakeCatalogClient.KeyboardId, quantity = 0 } } };

        using var response = await client.SendAsync(PlaceOrderRequest($"key-{Guid.NewGuid()}", body), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OtherCustomersOrderIsNotFound()
    {
        using var owner = ClientFor(Guid.NewGuid());
        using var stranger = ClientFor(Guid.NewGuid());
        using var placed = await owner.SendAsync(PlaceOrderRequest($"key-{Guid.NewGuid()}", TwoKeyboardsAndMonitor), Ct);

        using var response = await stranger.GetAsync(placed.Headers.Location, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

[Collection(OrdersApiTestGroup.Name)]
public sealed class CatalogUnavailableTests(OrdersApiFactory factory)
{
    [Fact]
    public async Task CatalogDownReturnsServiceUnavailable()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/orders")
        {
            Content = JsonContent.Create(new { items = new[] { new { productId = FakeCatalogClient.KeyboardId, quantity = 1 } } }),
        };
        request.Headers.Add("Idempotency-Key", $"key-{Guid.NewGuid()}");

        factory.Catalog.IsUnavailable = true;
        try
        {
            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
        finally
        {
            factory.Catalog.IsUnavailable = false;
        }
    }
}
