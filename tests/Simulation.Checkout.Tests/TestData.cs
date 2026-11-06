using System;
using Xunit;
using FluentAssertions;
using Moq;
using AutoFixture;
using Bogus;

public class DataTests
{
    /////Task 3/////


    [Fact]
    public async Task CheckoutAsync_WithAutoFixtureGeneratedOrder_Succeeds()
    {
        var fixture = new Fixture();
        var validOrder = fixture.Build<Order>()
     .With(o => o.Items, fixture.CreateMany<OrderItem>(2).Select(item => new OrderItem
     {
         ProductId = item.ProductId,
         ProductName = item.ProductName,
         Quantity = 2,
         UnitPrice = 100m
     }).ToList())
     .Create();

        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var receipt = await checkoutService.CheckoutAsync(validOrder);
        receipt.Should().NotBeNull();
    }




    static readonly Faker<Order> customerFaker = new Faker<Order>()
            .UseSeed(2026)
            .RuleFor(c => c.CustomerName, f => f.Name.FullName())
            .RuleFor(c => c.CustomerEmail, f => f.Internet.Email());

    [Fact]
    public async Task CheckoutAsync_BogusCustomerWithFixedSeed_ReceiptHasMatchingCustomerDetails()
    {
        var fakeCustomer = customerFaker.Generate();
        var order = new Order
        {
            CustomerName = fakeCustomer.CustomerName,
            CustomerEmail = fakeCustomer.CustomerEmail,
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Item",
                Quantity = 1,
                UnitPrice = 100m
            }
        }
        };

        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var receipt = await checkoutService.CheckoutAsync(order);
        receipt.CustomerName.Should().Be(fakeCustomer.CustomerName);
        receipt.CustomerEmail.Should().Be(fakeCustomer.CustomerEmail);

    }
}







