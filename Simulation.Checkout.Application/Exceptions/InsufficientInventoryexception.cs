namespace Simulation.Checkout.Application;

public sealed class InsufficientInventoryException : CheckoutException
{
    public InsufficientInventoryException(string productName)
        : base($"Insufficient inventory for '{productName}'.")
    {
    }
}