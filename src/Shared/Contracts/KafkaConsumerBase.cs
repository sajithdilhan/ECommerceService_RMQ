using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Common;
using System.Text.Json;

namespace Shared.Contracts;

public abstract class KafkaConsumerBase<T>(ILogger logger, IConfiguration config) : BackgroundService
{
    public abstract string Topic { get; }

    public abstract Task HandleMessageAsync(T? eventMessage);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(config[Constants.KafkaBootstrapServersConfigKey], nameof(Constants.KafkaConsumerGroupIdConfigKey));

        await Task.Run(async () =>
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config[Constants.KafkaBootstrapServersConfigKey],
                GroupId = config[Constants.KafkaConsumerGroupIdConfigKey],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(Topic);

            logger.LogInformation("Listening to topic {Topic}", Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var eventMessage = JsonSerializer.Deserialize<T>(cr.Message.Value);
                    await HandleMessageAsync(eventMessage);
                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }, stoppingToken);
    }
}