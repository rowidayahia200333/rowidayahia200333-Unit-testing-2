namespace Simulation.Checkout.Application;

public class CheckoutException : Exception
{
    public CheckoutException(string message)
        : base(message)
    {
    }
}