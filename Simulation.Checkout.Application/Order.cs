namespace Simulation.Checkout.Application;

public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string CustomerName { get; init; }

    public required string CustomerEmail { get; init; }

    public required IReadOnlyList<OrderItem> Items { get; init; }
}