namespace Orders.Domain;

/// <summary>
/// One line of an order. Name and unit price are a snapshot from Catalog at order time,
/// so history doesn't change when Catalog changes its prices (Chapter 1, §6).
/// </summary>
public sealed class OrderItem
{
    public const int MaxQuantity = 100;

    private OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("Product id is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        if (quantity is <= 0 or > MaxQuantity)
        {
            throw new DomainException($"Quantity must be between 1 and {MaxQuantity}.");
        }

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
