using Simulation.Checkout.Application;
using NetArchTest.Rules;
using Xunit;

namespace Simulation.Checkout.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void Domain_Should_Not_Have_Dependency_On_Application_Or_Infrastructure()
        {
            var result = Types.InAssembly(typeof(CheckoutService).Assembly)
                .Should()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void Application_Should_Not_Have_Dependency_On_Infrastructure()
        {
            var result = Types.InAssembly(typeof(CheckoutService).Assembly)
                .Should()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void Interfaces_Should_Start_With_I()
        {
            var result = Types.InAssembly(typeof(CheckoutService).Assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void OrderRepository_Implementations_Should_End_With_Repository()
        {
            var result = Types.InAssembly(typeof(CheckoutService).Assembly)
                .That()
                .ImplementInterface(typeof(IOrderRepository))
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            Assert.True(result.IsSuccessful);
        }
    }
}