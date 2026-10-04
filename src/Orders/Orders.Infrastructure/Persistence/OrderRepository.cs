using Microsoft.EntityFrameworkCore;
using Npgsql;
using Orders.Application.Orders;
using Orders.Domain;

namespace Orders.Infrastructure.Persistence;

internal sealed class OrderRepository(OrdersDbContext db) : IOrderRepository
{
    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

    public Task<Order?> FindByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken cancellationToken) =>
        db.Orders.AsNoTracking().FirstOrDefaultAsync(
            o => o.CustomerId == customerId
                && EF.Property<string>(o, OrdersDbContext.IdempotencyKeyProperty) == idempotencyKey,
            cancellationToken);

    public async Task AddAsync(Order order, string idempotencyKey, CancellationToken cancellationToken)
    {
        db.Orders.Add(order);
        db.Entry(order).Property(OrdersDbContext.IdempotencyKeyProperty).CurrentValue = idempotencyKey;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Forget the rejected order (and its owned items) so this DbContext can still be used
            db.ChangeTracker.Clear();
            throw new DuplicateIdempotencyKeyException("An order with this idempotency key already exists.", ex);
        }
    }
}
