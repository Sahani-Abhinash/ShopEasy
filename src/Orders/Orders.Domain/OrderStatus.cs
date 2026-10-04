namespace Orders.Domain;

/// <summary>
/// Order lifecycle. Transitions after <see cref="Pending"/> are driven by the checkout saga (Sprint 6, Chapter 5).
/// </summary>
public enum OrderStatus
{
    Pending,
    AwaitingPayment,
    ReleasingStock,
    Confirmed,
    Rejected,
}
