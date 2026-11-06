namespace Simulation.Checkout.Application;

public interface IInventoryService
{
    Task<bool> IsAvailableAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default);
}