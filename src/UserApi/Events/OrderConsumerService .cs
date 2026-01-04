using Shared.Common;
using Shared.Contracts;

namespace UserApi.Events;

public class OrderConsumerService(
    ILogger<OrderConsumerService> logger,
    IConfiguration config) : KafkaConsumerBase<OrderCreatedEvent>(logger: logger, config: config)
{
    public override string Topic => config[Constants.KafkaConsumerTopicConfigKey] ?? throw new ArgumentNullException("Kafka topic missing in config");

    public override async Task HandleMessageAsync(OrderCreatedEvent? eventMessage)
    {
        if (eventMessage == null) return;

        try
        {
            logger.LogInformation("Processed OrderCreated: {OrderId}, {Product}", eventMessage.Id, eventMessage.Product);
            // Possible to keep records of orders associated with users in User Service database, but for simplicity, just log the event here.
        }
        catch (Exception)
        {
            logger.LogError("Error processing OrderCreated: {OrderId}", eventMessage.Id);
            throw;
        }
    }
}