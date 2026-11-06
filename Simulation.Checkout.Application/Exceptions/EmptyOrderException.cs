namespace Simulation.Checkout.Application;

public sealed class EmptyOrderException : CheckoutException
{
    public EmptyOrderException()
        : base("An order must contain at least one item.")
    {
    }
}