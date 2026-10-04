using Orders.Domain;

namespace Orders.Application.Orders;

public sealed record PlaceOrderCommand(Guid CustomerId, string IdempotencyKey, IReadOnlyList<PlaceOrderLine> Items);

/// <summary>What the client may send: product and quantity. Never a price.</summary>
public sealed record PlaceOrderLine(Guid ProductId, int Quantity);

public abstract record PlaceOrderResult
{
    private PlaceOrderResult()
    {
    }

    /// <summary>Order accepted. <paramref name="IsExisting"/> is true when a retry returned an earlier order.</summary>
    public sealed record Accepted(Guid OrderId, OrderStatus Status, bool IsExisting) : PlaceOrderResult;

    public sealed record UnknownProducts(IReadOnlyList<Guid> ProductIds) : PlaceOrderResult;

    public sealed record Invalid(string Reason) : PlaceOrderResult;
}
