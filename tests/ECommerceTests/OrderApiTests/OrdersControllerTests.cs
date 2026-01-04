using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Controllers;
using OrderApi.Dtos;
using OrderApi.Services;
using Shared.Models;
using System.Text.Json;

namespace ECommerceTests.OrderApiTests;

public class OrdersControllerTests
{
    private readonly Mock<IOrdersService> _orderService;
    private readonly Mock<ILogger<OrdersController>> _logger;
    private readonly Mock<IDistributedCache> _cache;
    private readonly CancellationToken ct = CancellationToken.None;

    public OrdersControllerTests()
    {
        _orderService = new Mock<IOrdersService>();
        _logger = new Mock<ILogger<OrdersController>>();
        _cache = new Mock<IDistributedCache>();
    }

    [Fact]
    public async Task GetOrder_ReturnsOrder_WhenOrderExists()
    {
        // Arrange
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var expectedOrder = Result<OrderResponse>.Success(new OrderResponse { Id = orderId, UserId = userId, Product = "Product 1", Quantity = 1, Price = 38m });

        _orderService.Setup(s => s.GetOrderByIdAsync(orderId, ct)).ReturnsAsync(expectedOrder);

        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.GetOrder(orderId, ct);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var orderResult = Assert.IsType<OrderResponse>(okResult.Value);
        Assert.NotNull(orderResult);
        Assert.Equal(orderId, orderResult.Id);
        Assert.Equal(userId, orderResult.UserId);
        Assert.Equal("Product 1", orderResult.Product);
        Assert.Equal(1, orderResult.Quantity);
        Assert.Equal(38m, orderResult.Price);
    }

    [Fact]
    public async Task GetOrder_Returns_BadRequest_WhenOrderIdEmpty()
    {
        // Arrange
        Guid orderId = Guid.Empty;

        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.GetOrder(orderId, ct);

        // Assert
        Assert.NotNull(result);
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, problemResult.StatusCode);
        Assert.NotNull(problemResult.Value);
        Assert.Equal("Invalid order ID.", ((ProblemDetails)problemResult.Value).Detail);
    }

    [Fact]
    public async Task GetOrder_LogsWarning_WhenOrderIdEmpty()
    {
        // Arrange
        Guid orderId = Guid.Empty;
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);
        // Act
        var result = await controller.GetOrder(orderId, ct);
        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetOrder called with an empty GUID.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrder_WithValidId_CallsServiceOnce()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = Result<OrderResponse>.Success(new OrderResponse { Id = orderId, UserId = Guid.NewGuid() });
        _orderService.Setup(s => s.GetOrderByIdAsync(orderId, ct)).ReturnsAsync(expectedOrder);
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        await controller.GetOrder(orderId, ct);

        // Assert
        _orderService.Verify(s => s.GetOrderByIdAsync(orderId, ct), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenEmptyFields()
    {
        // Arrange

        var orderCreationRequest = new OrderCreationRequest
        {
            UserId = Guid.Empty,
            Product = "",
            Quantity = 0,
            Price = 0m
        };

        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderCreationRequest, ct);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, okResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderCreationRequest = new OrderCreationRequest
        {
            UserId = userId,
            Product = "Product X",
            Quantity = 1,
            Price = 0m
        };
        var orderId = Guid.NewGuid();
        var createdOrder = Result<OrderResponse>.Success(new OrderResponse
        {
            UserId = orderCreationRequest.UserId,
            Product = orderCreationRequest.Product,
            Quantity = orderCreationRequest.Quantity,
            Price = orderCreationRequest.Price,
            Id = orderId
        });

        _orderService.Setup(s => s.CreateOrderAsync(orderCreationRequest, ct)).ReturnsAsync(createdOrder);
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderCreationRequest, ct);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, okResult.StatusCode);
        var orderResult = Assert.IsType<OrderResponse>(okResult.Value);
        Assert.NotNull(orderResult);
        Assert.Equal(createdOrder.Value?.UserId, orderResult.UserId);
        Assert.Equal(createdOrder.Value?.Price, orderResult.Price);
        Assert.Equal(createdOrder.Value?.Product, orderResult.Product);
        Assert.Equal(createdOrder.Value?.Quantity, orderResult.Quantity);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenNullRequest()
    {
        // Arrange
        var newOrder = null as OrderCreationRequest;

        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(newOrder, ct);

        // Assert
        Assert.NotNull(result);
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, errorResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenNegativeQuantity()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = -1,
            Price = 10m
        };
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderRequest, ct);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenNegativePrice()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = 1,
            Price = -10m
        };
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderRequest, ct);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_LogsWarning_WhenValidationFails()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.Empty,
            Product = "",
            Quantity = 0,
            Price = 0m
        };
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        await controller.CreateOrder(orderRequest, ct);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateOrder called with invalid data")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithMinimumValidValues_ReturnsCreated()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "A",
            Quantity = 1,
            Price = 0.01m
        };
        var createdOrder = Result<OrderResponse>.Success(new OrderResponse { Id = Guid.NewGuid(), UserId = orderRequest.UserId });
        _orderService.Setup(s => s.CreateOrderAsync(orderRequest, ct)).ReturnsAsync(createdOrder);
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderRequest, ct);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithMaximumValues_ReturnsCreated()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = new string('X', 1000),
            Quantity = int.MaxValue,
            Price = decimal.MaxValue
        };
        var createdOrder = Result<OrderResponse>.Success(new OrderResponse { Id = Guid.NewGuid(), UserId = orderRequest.UserId });
        _orderService.Setup(s => s.CreateOrderAsync(orderRequest, ct)).ReturnsAsync(createdOrder);
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        var result = await controller.CreateOrder(orderRequest, ct);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_CallsServiceOnce()
    {
        // Arrange
        var orderRequest = new OrderCreationRequest
        {
            UserId = Guid.NewGuid(),
            Product = "Test",
            Quantity = 1,
            Price = 10m
        };
        var createdOrder = Result<OrderResponse>.Success(new OrderResponse { Id = Guid.NewGuid(), UserId = orderRequest.UserId });
        _orderService.Setup(s => s.CreateOrderAsync(orderRequest, ct)).ReturnsAsync(createdOrder);
        var controller = new OrdersController(_orderService.Object, _logger.Object, _cache.Object);

        // Act
        await controller.CreateOrder(orderRequest, ct);

        // Assert
        _orderService.Verify(s => s.CreateOrderAsync(orderRequest, ct), Times.Once);
    }
}
