namespace Simulation.Checkout.Application;

public sealed class InvalidPriceException : CheckoutException
{
    public InvalidPriceException(string productName)
        : base($"Unit price for '{productName}' cannot be negative.")
    {
    }
}