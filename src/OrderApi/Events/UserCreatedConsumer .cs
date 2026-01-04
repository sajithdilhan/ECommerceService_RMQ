using MassTransit;
using OrderApi.Services;
using Shared.Contracts;
using Shared.Models;

namespace OrderApi.Events;

public class UserCreatedConsumer(IOrdersService orderService, ILogger<UserCreatedConsumer> logger) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received UserCreatedEvent: UserId={UserId}, Name={Name}, Email={Email}",
            message.UserId, message.Name, message.Email);

        CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

        await orderService.CreateKnownUserAsync(new KnownUser
        {
            UserId = message.UserId,
            Email = message.Email
        }, cts.Token); 
    }
}