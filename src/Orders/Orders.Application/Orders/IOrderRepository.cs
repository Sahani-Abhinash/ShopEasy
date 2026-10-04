using Orders.Domain;

namespace Orders.Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Order?> FindByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>Saves a new order together with the idempotency key that created it.</summary>
    /// <exception cref="DuplicateIdempotencyKeyException">Another request with the same key was saved first.</exception>
    Task AddAsync(Order order, string idempotencyKey, CancellationToken cancellationToken);
}

/// <summary>The database unique index rejected a second order with the same key (concurrent retry).</summary>
public sealed class DuplicateIdempotencyKeyException : Exception
{
    public DuplicateIdempotencyKeyException()
    {
    }

    public DuplicateIdempotencyKeyException(string message)
        : base(message)
    {
    }

    public DuplicateIdempotencyKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
