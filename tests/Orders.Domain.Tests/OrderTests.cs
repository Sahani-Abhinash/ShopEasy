namespace Orders.Domain.Tests;

public sealed class OrderTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    private static OrderItem Keyboard(int quantity = 2) => new(Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1001"), "Keyboard", 89.99m, quantity);

    private static OrderItem Monitor(int quantity = 1) => new(Guid.Parse("b1f0c8a2-4c1e-4b7e-9a51-0f6d2c3a1003"), "Monitor", 349.00m, quantity);

    [Fact]
    public void CreateValidOrderIsPendingWithCorrectTotal()
    {
        var order = Order.Create(CustomerId, "EUR", [Keyboard(2), Monitor(1)], Now);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(CustomerId, order.CustomerId);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(528.98m, order.Total); // 2 × 89.99 + 349.00
        Assert.Equal(Now, order.CreatedAt);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void CreateWithoutItemsIsRejected()
    {
        var ex = Assert.Throws<DomainException>(() => Order.Create(CustomerId, "EUR", [], Now));
        Assert.Contains("at least one item", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(OrderItem.MaxQuantity + 1)]
    public void ItemWithInvalidQuantityIsRejected(int quantity)
    {
        Assert.Throws<DomainException>(() => Keyboard(quantity));
    }

    [Fact]
    public void ItemWithNegativePriceIsRejected()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.NewGuid(), "Broken", -1m, 1));
    }

    [Fact]
    public void SameProductTwiceIsRejected()
    {
        var ex = Assert.Throws<DomainException>(() => Order.Create(CustomerId, "EUR", [Keyboard(1), Keyboard(3)], Now));
        Assert.Contains("only once", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyCustomerIsRejected()
    {
        Assert.Throws<DomainException>(() => Order.Create(Guid.Empty, "EUR", [Keyboard()], Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("EURO")]
    public void InvalidCurrencyIsRejected(string currency)
    {
        Assert.Throws<DomainException>(() => Order.Create(CustomerId, currency, [Keyboard()], Now));
    }
}
