using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderApi.Events;
using OrderApi.Services;

namespace ECommerceTests.OrderApiTests;

public class UserConsumerServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<ILogger<UserConsumerService>> _logger;
    private readonly Mock<IOrdersService> _ordersService;
    private readonly Mock<IServiceScope> _serviceScope;

    public UserConsumerServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _serviceProvider = new Mock<IServiceProvider>();
        _logger = new Mock<ILogger<UserConsumerService>>();
        _ordersService = new Mock<IOrdersService>();
        _serviceScope = new Mock<IServiceScope>();
    }

    [Fact]
    public void Topic_ShouldReturnConfigValue_WhenConfigured()
    {
        // Arrange
        _configMock.Setup(c => c["Kafka:ConsumerTopic"]).Returns("user-created");
        var service = new UserConsumerService(_logger.Object, _configMock.Object, _serviceProvider.Object);

        // Assert
        Assert.Equal("user-created", service.Topic);
    }

    [Fact]
    public async Task HandleMessageAsync_WithNullEvent_ShouldReturnWithoutProcessing()
    {
        // Arrange
        var service = new UserConsumerService(_logger.Object, _configMock.Object, _serviceProvider.Object);

        // Act
        await service.HandleMessageAsync(null);

        // Assert
        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
