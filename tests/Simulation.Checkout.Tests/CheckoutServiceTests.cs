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






//classes


public sealed class OrderItem
    {
        public required Guid ProductId { get; init; }

        public required string ProductName { get; init; }

        public required decimal UnitPrice { get; init; }

        public required int Quantity { get; init; }
    }

    public sealed class Order
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string CustomerName { get; init; }

        public required string CustomerEmail { get; init; }

        public required IReadOnlyList<OrderItem> Items { get; init; }
    }
    public sealed class CheckoutService
    {
        public const decimal DiscountThreshold = 1000m;
        public const decimal DiscountRate = 0.10m;
        public const decimal MaxDiscount = 100m;

        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;

        public CheckoutService(
            IOrderRepository orderRepository,
            IInventoryService inventoryService,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<CheckoutReceipt> CheckoutAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(order);

            ValidateOrder(order);

            await EnsureInventoryAsync(order, cancellationToken);

            var subtotal = CalculateSubtotal(order);
            var discount = CalculateDiscount(subtotal);
            var total = subtotal - discount;
            var createdAt = DateTimeOffset.UtcNow;

            var receipt = new CheckoutReceipt
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Items = order.Items
                    .Select(item => new ReceiptLine
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        LineTotal = item.UnitPrice * item.Quantity
                    })
                    .ToList(),
                Subtotal = subtotal,
                Discount = discount,
                Total = total,
                CreatedAt = createdAt
            };

            await _orderRepository.SaveAsync(order, cancellationToken);
            await _notificationService.SendOrderConfirmationAsync(receipt, cancellationToken);

            return receipt;
        }

        private static void ValidateOrder(Order order)
        {
            if (order.Items is null || order.Items.Count == 0)
                throw new EmptyOrderException();

            foreach (var item in order.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidQuantityException(item.ProductName);

                if (item.UnitPrice < 0)
                    throw new InvalidPriceException(item.ProductName);
            }
        }

        private async Task EnsureInventoryAsync(Order order, CancellationToken cancellationToken)
        {
            foreach (var item in order.Items)
            {
                var available = await _inventoryService.IsAvailableAsync(
                    item.ProductId,
                    item.Quantity,
                    cancellationToken);

                if (!available)
                    throw new InsufficientInventoryException(item.ProductName);
            }
        }

        private static decimal CalculateSubtotal(Order order) =>
            order.Items.Sum(item => item.UnitPrice * item.Quantity);

        internal static decimal CalculateDiscount(decimal subtotal)
        {
            if (subtotal < DiscountThreshold)
                return 0m;

            var discount = decimal.Round(subtotal * DiscountRate, 2, MidpointRounding.AwayFromZero);
            return discount > MaxDiscount ? MaxDiscount : discount;
        }
    }
    public interface IOrderRepository
    {
        Task SaveAsync(Order order, CancellationToken cancellationToken = default);
    }
    public interface INotificationService
    {
        Task SendOrderConfirmationAsync(
            CheckoutReceipt receipt,
            CancellationToken cancellationToken = default);
    }
    public interface IInventoryService
    {
        Task<bool> IsAvailableAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);
    }
    public sealed class CheckoutReceipt
    {
        public required Guid OrderId { get; init; }

        public required string CustomerName { get; init; }

        public required string CustomerEmail { get; init; }

        public required IReadOnlyList<ReceiptLine> Items { get; init; }

        public required decimal Subtotal { get; init; }

        public required decimal Discount { get; init; }

        public required decimal Total { get; init; }

        public required DateTimeOffset CreatedAt { get; init; }
    }

    public sealed class ReceiptLine
    {
        public required Guid ProductId { get; init; }

        public required string ProductName { get; init; }

        public required decimal UnitPrice { get; init; }

        public required int Quantity { get; init; }

        public required decimal LineTotal { get; init; }
    }
    public class CheckoutException : Exception
    {
        public CheckoutException(string message)
            : base(message)
        {
        }
    }

    public sealed class EmptyOrderException : CheckoutException
    {
        public EmptyOrderException()
            : base("An order must contain at least one item.")
        {
        }
    }

    public sealed class InvalidQuantityException : CheckoutException
    {
        public InvalidQuantityException(string productName)
            : base($"Quantity for '{productName}' must be greater than zero.")
        {
        }
    }

    public sealed class InvalidPriceException : CheckoutException
    {
        public InvalidPriceException(string productName)
            : base($"Unit price for '{productName}' cannot be negative.")
        {
        }
    }

    public sealed class InsufficientInventoryException : CheckoutException
    {
        public InsufficientInventoryException(string productName)
            : base($"Insufficient inventory for '{productName}'.")
        {
        }
    }



