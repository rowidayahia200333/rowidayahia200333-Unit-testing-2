namespace Simulation.Checkout.Application;

public sealed class ReceiptLine
{
    public required Guid ProductId { get; init; }

    public required string ProductName { get; init; }

    public required decimal UnitPrice { get; init; }

    public required int Quantity { get; init; }

    public required decimal LineTotal { get; init; }
}