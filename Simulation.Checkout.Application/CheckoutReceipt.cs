namespace Simulation.Checkout.Application;

public sealed class CheckoutReceipt
{
    public required Guid OrderId { get; init; }

    public required string CustomerName { get; init; }

    public required string CustomerEmail { get; init; }

    public required IReadOnlyList<ReceiptLine> Items { get; init; }

    public required decimal Subtotal { get; init; }

    public required decimal Discount { get; init; }

    public required decimal Total { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}