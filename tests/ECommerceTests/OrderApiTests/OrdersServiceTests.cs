using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Data;
using OrderApi.Dtos;
using OrderApi.Services;
using Shared.Contracts;
using Shared.Exceptions;
using Shared.Models;

namespace ECommerceTests.OrderApiTests;

public class OrdersServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository;
    private readonly Mock<ILogger<OrdersService>> _logger;
    private CancellationToken _cancellationToken = CancellationToken.None;


    public OrdersServiceTests()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _logger = new Mock<ILogger<OrdersService>>();
    }

    [Fact]
    public async Task CreateOrder_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var newOrderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = 2,
            Price = 100
        };

        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(null as KnownUser);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.CreateOrderAsync(newOrderRequest, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains($"Attempted to create order for unknown user ID {newOrderRequest.UserId}", result.Error?.Message);
    }

    [Fact]
    public async Task CreateOrder_Returns_CreatedOrder()
    {
        // Arrange
        var newOrderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = 2,
            Price = 100
        };
        var knownUser = new KnownUser
        {
            UserId = newOrderRequest.UserId,
            Email = "sajith@mail.com"
        };
        var orderId = Guid.NewGuid();
        var createdOrder = new Order
        {
            Id = orderId,
            UserId = newOrderRequest.UserId,
            Product = newOrderRequest.Product,
            Quantity = newOrderRequest.Quantity,
            Price = newOrderRequest.Price
        };

        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(),_cancellationToken))
            .ReturnsAsync(knownUser);

        _orderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>(), _cancellationToken))
            .ReturnsAsync(createdOrder);


        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.CreateOrderAsync(newOrderRequest, _cancellationToken);

        // Assert   
        Assert.NotNull(result);
        Assert.Equal(createdOrder.Id, result.Value?.Id);
    }

    [Fact]
    public async Task CreateOrder_ReturnsFailure_WhenRepositoryFails()
    {
        // Arrange
        var newOrderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = 2,
            Price = 100
        };
        var knownUser = new KnownUser { UserId = newOrderRequest.UserId };

        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(knownUser);
        _orderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>(), _cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.CreateOrderAsync(newOrderRequest, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Order creation failed.", result.Error?.Message);
    }

    [Fact]
    public async Task CreateOrder_WithMinimumValues_ReturnsCreatedOrder()
    {
        // Arrange
        var newOrderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "A",
            Quantity = 1,
            Price = 0.01m
        };
        var knownUser = new KnownUser { UserId = newOrderRequest.UserId };
        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = newOrderRequest.UserId,
            Product = newOrderRequest.Product,
            Quantity = newOrderRequest.Quantity,
            Price = newOrderRequest.Price
        };

        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(knownUser);
        _orderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>(), _cancellationToken))
            .ReturnsAsync(createdOrder);
       
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.CreateOrderAsync(newOrderRequest, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdOrder.Id, result.Value?.Id);
        Assert.Equal(newOrderRequest.Product, result.Value?.Product);
        Assert.Equal(newOrderRequest.Quantity, result.Value?.Quantity);
        Assert.Equal(newOrderRequest.Price, result.Value?.Price);
    }

    [Fact]
    public async Task CreateOrder_WithLargeValues_ReturnsCreatedOrder()
    {
        // Arrange
        var newOrderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = new string('A', 1000),
            Quantity = int.MaxValue,
            Price = decimal.MaxValue
        };
        var knownUser = new KnownUser { UserId = newOrderRequest.UserId };
        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = newOrderRequest.UserId,
            Product = newOrderRequest.Product,
            Quantity = newOrderRequest.Quantity,
            Price = newOrderRequest.Price
        };

        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(knownUser);
        _orderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>(), _cancellationToken))
            .ReturnsAsync(createdOrder);

        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.CreateOrderAsync(newOrderRequest, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdOrder.Id, result.Value?.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReurnsOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync(new Order
            {
                UserId = new Guid(),
                Id = orderId,
                Price = 100,
                Product = "Test Product",
                Quantity = 2
            });

        var ordersService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await ordersService.GetOrderByIdAsync(orderId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Value?.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ThrowsException_WhenDb_Exception()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId, _cancellationToken)).ThrowsAsync(new Exception("Database error"));

        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);


        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => orderService.GetOrderByIdAsync(orderId, _cancellationToken)
        );

        Assert.Equal("Database error", ex.Message);
        _orderRepository.Verify(r => r.GetOrderByIdAsync(It.IsAny<Guid>(), _cancellationToken), Times.Once);

    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsOrderWithAllProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            UserId = userId,
            Product = "Test Product",
            Quantity = 5,
            Price = 99.99m
        };

        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId, _cancellationToken)).ReturnsAsync(expectedOrder);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.GetOrderByIdAsync(orderId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.Id, result.Value?.Id);
        Assert.Equal(expectedOrder.UserId, result.Value?.UserId);
        Assert.Equal(expectedOrder.Product, result.Value?.Product);
        Assert.Equal(expectedOrder.Quantity, result.Value?.Quantity);
        Assert.Equal(expectedOrder.Price, result.Value?.Price);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithMinimumValues_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Product = "P",
            Quantity = 1,
            Price = 0.01m
        };

        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId, _cancellationToken)).ReturnsAsync(expectedOrder);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.GetOrderByIdAsync(orderId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.Id, result.Value?.Id);
        Assert.Equal(expectedOrder.Product, result.Value?.Product);
        Assert.Equal(expectedOrder.Quantity, result.Value?.Quantity);
        Assert.Equal(expectedOrder.Price, result.Value?.Price);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithMaximumValues_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Product = new string('X', 1000),
            Quantity = int.MaxValue,
            Price = decimal.MaxValue
        };

        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId, _cancellationToken)).ReturnsAsync(expectedOrder);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        var result = await orderService.GetOrderByIdAsync(orderId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.Id, result.Value?.Id);
        Assert.Equal(expectedOrder.Product, result.Value?.Product);
        Assert.Equal(expectedOrder.Quantity, result.Value?.Quantity);
        Assert.Equal(expectedOrder.Price, result.Value?.Price);
    }

    [Fact]
    public async Task GetOrderByIdAsync_CallsRepositoryOnce()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order { Id = orderId, UserId = Guid.NewGuid(), Product = "Test", Quantity = 1, Price = 10 };
        _orderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId, _cancellationToken)).ReturnsAsync(expectedOrder);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        await orderService.GetOrderByIdAsync(orderId, _cancellationToken);

        // Assert
        _orderRepository.Verify(r => r.GetOrderByIdAsync(orderId, _cancellationToken), Times.Once);
        _orderRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateKnownUserAsync_WhenUserExists_ReturnsExistingUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var knownUser = new KnownUser { UserId = userId };
        var existingUser = new KnownUser { UserId = userId };

        _orderRepository
            .Setup(repo => repo.GetKnownUserByIdAsync(knownUser.UserId, _cancellationToken))
            .ReturnsAsync(existingUser);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);
        // Act
        await orderService.CreateKnownUserAsync(knownUser, _cancellationToken);

        // Assert
        _orderRepository.Verify(repo => repo.CreateKnownUserAsync(It.IsAny<KnownUser>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task CreateKnownUserAsync_WhenUserDoesNotExist_CreatesAndReturnsNewUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var knownUser = new KnownUser { UserId = userId };
        var createdUser = new KnownUser { UserId = userId };

        _orderRepository
            .Setup(repo => repo.GetKnownUserByIdAsync(knownUser.UserId, _cancellationToken))
            .ReturnsAsync(null as KnownUser);

        _orderRepository
            .Setup(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken))
            .ReturnsAsync(1);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        await orderService.CreateKnownUserAsync(knownUser, _cancellationToken);

        // Assert
        _orderRepository.Verify(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateKnownUserAsync_WithEmptyGuid_DoesNotCreateUser()
    {
        // Arrange
        var knownUser = new KnownUser { UserId = Guid.Empty };
        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(Guid.Empty, _cancellationToken)).ReturnsAsync((KnownUser?)null);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        await orderService.CreateKnownUserAsync(knownUser, _cancellationToken);

        // Assert
        _orderRepository.Verify(repo => repo.GetKnownUserByIdAsync(Guid.Empty, _cancellationToken), Times.Once);
        _orderRepository.Verify(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateKnownUserAsync_ThrowsException_WhenRepositoryGetFails()
    {
        // Arrange
        var knownUser = new KnownUser { UserId = Guid.NewGuid() };
        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ThrowsAsync(new Exception("Database error"));
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => orderService.CreateKnownUserAsync(knownUser, _cancellationToken));
        Assert.Equal("Database error", ex.Message);
    }

    [Fact]
    public async Task CreateKnownUserAsync_ThrowsException_WhenRepositoryCreateFails()
    {
        // Arrange
        var knownUser = new KnownUser { UserId = Guid.NewGuid() };
        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(It.IsAny<Guid>(), _cancellationToken))
            .ReturnsAsync((KnownUser?)null);
        _orderRepository.Setup(repo => repo.CreateKnownUserAsync(It.IsAny<KnownUser>(), _cancellationToken))
            .ThrowsAsync(new Exception("Create failed"));
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => orderService.CreateKnownUserAsync(knownUser, _cancellationToken));
        Assert.Equal("Create failed", ex.Message);
    }

    [Fact]
    public async Task CreateKnownUserAsync_WithCompleteUserData_CreatesUser()
    {
        // Arrange
        var knownUser = new KnownUser
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com"
        };
        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(knownUser.UserId, _cancellationToken))
            .ReturnsAsync((KnownUser?)null);
        _orderRepository.Setup(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken))
            .ReturnsAsync(1);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        await orderService.CreateKnownUserAsync(knownUser, _cancellationToken);

        // Assert
        _orderRepository.Verify(repo => repo.CreateKnownUserAsync(It.Is<KnownUser>(u =>
            u.UserId == knownUser.UserId && u.Email == knownUser.Email), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateKnownUserAsync_CallsGetBeforeCreate()
    {
        // Arrange
        var knownUser = new KnownUser { UserId = Guid.NewGuid() };
        _orderRepository.Setup(repo => repo.GetKnownUserByIdAsync(knownUser.UserId, _cancellationToken))
            .ReturnsAsync((KnownUser?)null);
        _orderRepository.Setup(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken))
            .ReturnsAsync(1);
        var orderService = new OrdersService(_orderRepository.Object, _logger.Object);

        // Act
        await orderService.CreateKnownUserAsync(knownUser, _cancellationToken);

        // Assert
        _orderRepository.Verify(repo => repo.GetKnownUserByIdAsync(knownUser.UserId, _cancellationToken), Times.Once);
        _orderRepository.Verify(repo => repo.CreateKnownUserAsync(knownUser, _cancellationToken), Times.Once);
    }
}