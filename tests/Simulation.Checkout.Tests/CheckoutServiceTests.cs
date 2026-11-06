using Simulation.Checkout.Application;
using System;
using Xunit;
using FluentAssertions;
using Moq;
using AutoFixture;
using Bogus;


public class CheckoutServiceTests
{
    [Fact]
    public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var checkoutService = new CheckoutService(
             orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        //action
        var emptyOrder = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>()
        };

        //assert
        var exception = await Assert.ThrowsAsync<EmptyOrderException>(() => checkoutService.CheckoutAsync(emptyOrder));

        exception.Should().NotBeNull();

    }

    [Fact]
    public async Task CheckoutAsync_WithInvalidQuantity_ThrowsInvalidQuantityException()
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var invalidOrder = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
           {
            new OrderItem
            {
                ProductId= Guid.NewGuid(),
                ProductName = "Laptop",
                Quantity = 0,
                UnitPrice = 1000
            }
           }
        };
        var exception = await Assert.ThrowsAsync<InvalidQuantityException>(() => checkoutService.CheckoutAsync(invalidOrder));
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Laptop");
    }
    [Fact]
    public async Task CheckoutAsync_WithInvalidPrice_ThrowsInvalidPriceException()
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var checkoutService = new CheckoutService(
      orderRepositoryMock.Object,
      inventoryServiceMock.Object,
      notificationServiceMock.Object);


        var invalidOrder = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Mouse",
                    Quantity = 2,
                    UnitPrice = -50
                }
            }
        };

        var exception = await Assert.ThrowsAsync<InvalidPriceException>(() => checkoutService.CheckoutAsync(invalidOrder));
        exception.Should().NotBeNull();

    }

    [Fact]
    public async Task CheckoutAsync_WhenItemOutOfStock_ThrowsInsufficientInventoryException()
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
        .Setup(x => x.IsAvailableAsync(Guid.NewGuid(), 1))
        .ReturnsAsync(false);

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var order = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Keyboard",
                Quantity = 1,
                UnitPrice = 500
            }
        }
        };

        var exception = await Assert.ThrowsAsync<InsufficientInventoryException>(() => checkoutService.CheckoutAsync(order));
        exception.Should().NotBeNull();
    }
    [Fact]
    public async Task CheckoutAsync_WithValidOrderAndDiscount_CalculatesTotalsAndSavesOrder()
    {
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


        var validOrder = new Order
        {
            CustomerName = "Rowida Yahia",
            CustomerEmail = "rowida@example.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Monitor",
                Quantity = 3,
                UnitPrice = 500 //subtotal= 1500
            }
        }
        };

        var receipt = await checkoutService.CheckoutAsync(validOrder);

        receipt.Should().NotBeNull();
        receipt.Subtotal.Should().Be(1500);
        receipt.Discount.Should().Be(100);
        receipt.Total.Should().Be(1400);

        orderRepositoryMock.Verify(x => x.SaveAsync(validOrder), Times.Once);
        notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(receipt), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_CalculatesSubtotalCorrectly_BasedOnItems()
    {
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


        //(2*150)+(1*400)=700
        var order = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Item 1",
                Quantity = 2,
                UnitPrice = 150
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Item 2",
                Quantity = 1,
                UnitPrice = 400
            }
        }
        };
        var receipt = await checkoutService.CheckoutAsync(order);
        receipt.Should().NotBeNull();
        receipt.Subtotal.Should().Be(700);

    }

    [Fact]
    public async Task CheckoutAsync_CalculatesTotalCorrectly_AsSubtotalMinusDiscount()
    {
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

        var order = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                Quantity = 3,
                UnitPrice = 500m
            }
        }
        };

        var receipt = await checkoutService.CheckoutAsync(order);
        receipt.Total.Should().Be(receipt.Subtotal - receipt.Discount); //1500- 100 = 1400
    }

    [Fact]
    public async Task CheckoutAsync_OnSuccess_SavesOrderOnceAndSendsSingleConfirmation()
    {
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

        var validOrder = new Order
        {
            CustomerName = "Rowida Yahia",
            CustomerEmail = "rowida@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Keyboard",
                Quantity = 1,
                UnitPrice = 500m
            }
        }
        };

        var receipt = await checkoutService.CheckoutAsync(validOrder);
        receipt.Should().NotBeNull();

        orderRepositoryMock.Verify(
            x => x.SaveAsync(validOrder, It.IsAny<CancellationToken>()),
            Times.Once
        );

        notificationServiceMock.Verify(
            x => x.SendOrderConfirmationAsync(receipt),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckoutAsync_OnAnyFailure_DoesNotSaveOrderAndSendsNoNotification()
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var invalidOrder = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Faulty Item",
                Quantity = 0,  //to fail
                UnitPrice = 100
            }
        }
        };

        var exception = await Assert.ThrowsAsync<InvalidQuantityException>(() => checkoutService.CheckoutAsync(invalidOrder));
        orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
        notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()), Times.Never);

    }




    /////Theory/////


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]

    public async Task CheckoutAsync_InvalidQuantity_ThrowsInvalidQuantityException(int invalidQuantity)
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var checkoutService = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object
        );

        var invalidOrder = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Item",
                Quantity = invalidQuantity,
                UnitPrice = 100
            }
        }
        };

        await Assert.ThrowsAsync<InvalidQuantityException>(() => checkoutService.CheckoutAsync(invalidOrder));

    }



    public static TheoryData<decimal, decimal> DiscountTestData => new()
    {
        {999.99m, 0m },
         {1000.00m, 100m},
         {1500.00m, 100m}
    };

    [Theory]
    [MemberData(nameof(DiscountTestData))]

    public async Task CheckoutAsync_DiscountThresholds_AppliesCorrectDiscount(decimal subtotal, decimal expectedDiscount)
    {
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

        var order = new Order
        {
            CustomerName = "Test User",
            CustomerEmail = "test@test.com",
            Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Item",
                Quantity = 1,
                UnitPrice = subtotal
            }
        }
        };

        var receipt = await checkoutService.CheckoutAsync(order);
        receipt.Discount.Should().Be(expectedDiscount);

    }



}

