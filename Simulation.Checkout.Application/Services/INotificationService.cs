namespace Simulation.Checkout.Application;

public interface INotificationService
{
    Task SendOrderConfirmationAsync(
        CheckoutReceipt receipt,
        CancellationToken cancellationToken = default);
}
