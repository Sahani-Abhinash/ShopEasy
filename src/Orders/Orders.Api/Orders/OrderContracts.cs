using Orders.Domain;

namespace Orders.Api.Orders;

/// <summary>Request body. Deliberately has no price: prices come only from Catalog.</summary>
internal sealed record PlaceOrderRequest(IReadOnlyList<PlaceOrderRequestItem>? Items);

internal sealed record PlaceOrderRequestItem(Guid ProductId, int Quantity);

internal sealed record PlaceOrderResponse(Guid OrderId, string Status);

internal sealed record OrderResponse(
    Guid Id,
    string Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemResponse> Items)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.Status.ToString(),
        order.Currency,
        order.Total,
        order.CreatedAt,
        [.. order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal))]);
}

internal sealed record OrderItemResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
