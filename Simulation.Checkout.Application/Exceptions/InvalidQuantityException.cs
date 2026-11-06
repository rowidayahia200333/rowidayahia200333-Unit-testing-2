namespace Simulation.Checkout.Application;

public sealed class InvalidQuantityException : CheckoutException
{
    public InvalidQuantityException(string productName)
        : base($"Quantity for '{productName}' must be greater than zero.")
    {
    }
}
