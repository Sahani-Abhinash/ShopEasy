namespace Orders.Domain;

/// <summary>
/// Aggregate root: the only way to create or change an order, so its rules always hold (Chapter 2, §6).
/// </summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
        Currency = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public string Currency { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;

    public decimal Total => _items.Sum(i => i.LineTotal);

    public static Order Create(Guid customerId, string currency, IEnumerable<OrderItem> items, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer id is required.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter code.");
        }

        var list = items.ToList();

        if (list.Count == 0)
        {
            throw new DomainException("An order needs at least one item.");
        }

        if (list.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
        {
            throw new DomainException("Each product can appear only once in an order.");
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(createdAt),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Currency = currency,
            CreatedAt = createdAt,
        };
        order._items.AddRange(list);
        return order;
    }

    // Status changes (OnStockReserved, OnPaymentSucceeded, ...) are added in Sprint 6.
}
