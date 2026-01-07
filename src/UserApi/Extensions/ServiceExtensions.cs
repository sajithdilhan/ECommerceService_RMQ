using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MassTransit;
using Shared.Models;
using System.Text.Json;

namespace UserApi.Extensions;

public static class ServiceExtensions
{
    public static async Task AddRabbitMqMassTransitAsync(this WebApplicationBuilder builder, IAmazonSecretsManager secretsManager)
    {
        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger("RabbitMqSetup");

        try
        {
            var baseRabbitSettings = builder.Configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();

            if (baseRabbitSettings is null
                || string.IsNullOrWhiteSpace(baseRabbitSettings.Host)
                || string.IsNullOrWhiteSpace(baseRabbitSettings.VirtualHost))
            {
                logger.LogError("RabbitMq base settings are not properly configured.");
                throw new InvalidOperationException("RabbitMq base settings are not properly configured.");
            }

            var result = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = "RabbitMQ" });

            var secretRabbitSettings = JsonSerializer.Deserialize<RabbitMqSettings>(result.SecretString);

            if (secretRabbitSettings is null
                || string.IsNullOrWhiteSpace(secretRabbitSettings.Username)
                || string.IsNullOrWhiteSpace(secretRabbitSettings.Password))
            {
                logger.LogError("RabbitMq secrets are not properly configured.");
                throw new InvalidOperationException("RabbitMq secrets are not properly configured.");
            }


            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(baseRabbitSettings.Host, baseRabbitSettings.VirtualHost, h =>
                    {
                        h.Username(secretRabbitSettings.Username);
                        h.Password(secretRabbitSettings.Password);
                    });
                });
            });
            logger.LogInformation("RabbitMq MassTransit configured successfully.");
        }
        catch (Exception)
        {
            logger.LogError("Failed to configure RabbitMq MassTransit.");
            throw;
        }
    }
}