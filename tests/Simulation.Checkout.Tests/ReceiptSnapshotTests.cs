using VerifyXunit;
using System;
using Xunit;
using FluentAssertions;
using Moq;
using AutoFixture;
using Bogus;



/////Task 4 /////

public class ReceiptSnapshotTests
{
    [Fact]
    public async Task CheckoutAsync_ExampleC_MatchesSnapshot()
    {
        var order = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "Wireless Mouse",
                Quantity = 2,
                UnitPrice = 150m
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
        await Verify(receipt);
    }
}




