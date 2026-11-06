namespace Simulation.Checkout.Application;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}