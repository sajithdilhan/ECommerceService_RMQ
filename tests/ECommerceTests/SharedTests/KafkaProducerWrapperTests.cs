using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts;

namespace ECommerceTests.SharedTests;

public class KafkaProducerWrapperTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<KafkaProducerWrapper>> _loggerMock;

    public KafkaProducerWrapperTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<KafkaProducerWrapper>>();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldInitialize()
    {
        // Arrange
        _configMock.Setup(c => c["Kafka:ProducerTopic"]).Returns("test-topic");
        _configMock.Setup(c => c["Kafka:BootstrapServers"]).Returns("localhost:9092");

        // Act & Assert
        var producer = new KafkaProducerWrapper(_configMock.Object, _loggerMock.Object);
        Assert.NotNull(producer);
    }
}