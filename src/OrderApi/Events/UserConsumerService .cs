using OrderApi.Services;
using Shared.Common;
using Shared.Contracts;

namespace OrderApi.Events;

public class UserConsumerService(
    ILogger<UserConsumerService> logger, 
    IConfiguration config, 
    IServiceProvider serviceProvider) : KafkaConsumerBase<UserCreatedEvent>(logger: logger, config: config)
{

    public override string Topic => config[Constants.KafkaConsumerTopicConfigKey] ?? throw new ArgumentNullException("Kafka topic missing in config");

    public override async Task HandleMessageAsync(UserCreatedEvent? eventMessage)
    {
        if (eventMessage == null) return;

        try
        {
            using var scope = serviceProvider.CreateScope();
            var ordersService = scope.ServiceProvider.GetRequiredService<IOrdersService>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            await ordersService.CreateKnownUserAsync(
                new Shared.Models.KnownUser() { UserId = eventMessage.UserId, Email = eventMessage.Email }, cts);

            logger.LogInformation("Processed UserCreated: {UserId}, {Name}", eventMessage.UserId, eventMessage.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing UserCreatedEvent for UserId: {UserId}", eventMessage.UserId);
            // implement retry logic here
        }
    }
}