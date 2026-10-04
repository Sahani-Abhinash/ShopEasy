using Orders.Application.Catalog;
using Orders.Domain;

namespace Orders.Application.Orders;

/// <summary>Use case: place an order (Chapter 2, §7).</summary>
public sealed class PlaceOrderHandler(IOrderRepository orders, ICatalogClient catalog, TimeProvider timeProvider)
{
    public async Task<PlaceOrderResult> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. Idempotency: the same key from the same customer returns the same order
        var existing = await orders.FindByIdempotencyKeyAsync(command.CustomerId, command.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return new PlaceOrderResult.Accepted(existing.Id, existing.Status, IsExisting: true);
        }

        // 2. Authoritative names and prices come from Catalog, never from the client
        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await catalog.GetProductsAsync(productIds, cancellationToken);

        var missing = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missing.Count > 0)
        {
            return new PlaceOrderResult.UnknownProducts(missing);
        }

        var currencies = products.Select(p => p.Currency).Distinct().ToList();
        if (currencies.Count != 1)
        {
            return new PlaceOrderResult.Invalid("All products in an order must have the same currency.");
        }

        // 3. The domain decides whether the order is valid
        Order order;
        try
        {
            var items = command.Items.Select(line =>
            {
                var product = products.First(p => p.Id == line.ProductId);
                return new OrderItem(product.Id, product.Name, product.Price, line.Quantity);
            });

            order = Order.Create(command.CustomerId, currencies[0], items, timeProvider.GetUtcNow());
        }
        catch (DomainException ex)
        {
            return new PlaceOrderResult.Invalid(ex.Message);
        }

        // 4. Persist. A concurrent request with the same key may have won the race:
        //    the unique index rejects ours, and we return the winner's order.
        try
        {
            await orders.AddAsync(order, command.IdempotencyKey, cancellationToken);
        }
        catch (DuplicateIdempotencyKeyException)
        {
            var winner = await orders.FindByIdempotencyKeyAsync(command.CustomerId, command.IdempotencyKey, cancellationToken)
                ?? throw new InvalidOperationException("Duplicate idempotency key reported, but no order was found.");
            return new PlaceOrderResult.Accepted(winner.Id, winner.Status, IsExisting: true);
        }

        // An outbox message (ReserveStock) is added here in Sprint 4
        return new PlaceOrderResult.Accepted(order.Id, order.Status, IsExisting: false);
    }
}
