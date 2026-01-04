using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;
using UserApi.Events;
using Shared.Common;

namespace ECommerceTests.UserApiTests;

public class OrderConsumerServiceTests
{
    [Fact]
    public void Topic_ShouldReturnConfigValue_WhenConfigured()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c[Constants.KafkaConsumerTopicConfigKey]).Returns("order-created");

        var loggerMock = new Mock<ILogger<OrderConsumerService>>();
        var service = new OrderConsumerService(loggerMock.Object, configMock.Object);

        // Assert
        Assert.Equal("order-created", service.Topic);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldLogInformation()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Kafka:ConsumerTopic"]).Returns("order-created");

        var loggerMock = new Mock<ILogger<OrderConsumerService>>();
        var service = new OrderConsumerService(loggerMock.Object, configMock.Object);

        var orderId = Guid.NewGuid();
        var @event = new OrderCreatedEvent
        {
            Id = orderId,
            Product = "Test Product",
            Quantity = 2,
            Price = 19.99m,
            UserId = Guid.NewGuid()
        };

        // Act
        await service.HandleMessageAsync(@event);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Processed OrderCreated: {orderId}, Test Product")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_WithNullEvent_ShouldReturnWithoutLogging()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<OrderConsumerService>>();
        var service = new OrderConsumerService(loggerMock.Object, configMock.Object);

        // Act
        await service.HandleMessageAsync(null);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_WithValidEvent_LogsCorrectInformation()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<OrderConsumerService>>();
        var service = new OrderConsumerService(loggerMock.Object, configMock.Object);

        var orderId = Guid.NewGuid();
        var @event = new OrderCreatedEvent
        {
            Id = orderId,
            Product = "Sample Product",
            Quantity = 5,
            Price = 99.99m,
            UserId = Guid.NewGuid()
        };

        // Act
        await service.HandleMessageAsync(@event);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processed OrderCreated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_CompletesSuccessfully()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<OrderConsumerService>>();
        var service = new OrderConsumerService(loggerMock.Object, configMock.Object);

        var @event = new OrderCreatedEvent
        {
            Id = Guid.NewGuid(),
            Product = "Test Product",
            Quantity = 1,
            Price = 10m,
            UserId = Guid.NewGuid()
        };

        // Act & Assert - Should not throw
        await service.HandleMessageAsync(@event);
    }
}