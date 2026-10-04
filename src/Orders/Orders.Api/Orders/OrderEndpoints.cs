using Microsoft.AspNetCore.Mvc;
using Orders.Api.Customers;
using Orders.Application.Catalog;
using Orders.Application.Orders;

namespace Orders.Api.Orders;

internal static class OrderEndpoints
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";
    private const int MaxIdempotencyKeyLength = 100;

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/v1/orders").WithTags("Orders");

        orders.MapPost("/", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place an order. Send a unique Idempotency-Key header per checkout attempt; retries reuse it.");

        orders.MapGet("/{id:guid}", GetOrderAsync)
            .WithName("GetOrder")
            .WithSummary("Get one of your own orders");

        return app;
    }

    private static async Task<IResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        ICurrentCustomer customer,
        PlaceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > MaxIdempotencyKeyLength)
        {
            return TypedResults.Problem(
                detail: $"The {IdempotencyKeyHeader} header is required (max {MaxIdempotencyKeyLength} characters).",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.Items is not { Count: > 0 })
        {
            return TypedResults.Problem(detail: "An order needs at least one item.", statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new PlaceOrderCommand(
            customer.Id,
            idempotencyKey,
            [.. request.Items.Select(i => new PlaceOrderLine(i.ProductId, i.Quantity))]);

        PlaceOrderResult result;
        try
        {
            result = await handler.HandleAsync(command, cancellationToken);
        }
        catch (CatalogUnavailableException)
        {
            return TypedResults.Problem(
                detail: "Prices can't be checked right now. Please retry with the same Idempotency-Key.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return result switch
        {
            // 202: "we accepted your request"; the checkout continues in the background (Chapter 1, §8)
            PlaceOrderResult.Accepted accepted => TypedResults.Accepted(
                $"/api/v1/orders/{accepted.OrderId}",
                new PlaceOrderResponse(accepted.OrderId, accepted.Status.ToString())),

            PlaceOrderResult.UnknownProducts unknown => TypedResults.Problem(
                detail: "Some products don't exist.",
                statusCode: StatusCodes.Status422UnprocessableEntity,
                extensions: new Dictionary<string, object?> { ["unknownProductIds"] = unknown.ProductIds }),

            PlaceOrderResult.Invalid invalid => TypedResults.Problem(
                detail: invalid.Reason,
                statusCode: StatusCodes.Status400BadRequest),

            _ => throw new InvalidOperationException($"Unhandled result {result.GetType().Name}."),
        };
    }

    private static async Task<IResult> GetOrderAsync(
        Guid id,
        ICurrentCustomer customer,
        IOrderRepository orders,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(id, cancellationToken);

        // Someone else's order → 404, not 403: don't reveal that it exists
        return order is null || order.CustomerId != customer.Id
            ? TypedResults.NotFound()
            : TypedResults.Ok(OrderResponse.From(order));
    }
}
