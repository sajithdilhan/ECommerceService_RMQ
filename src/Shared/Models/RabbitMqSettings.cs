namespace Shared.Models;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMqSettings";
    public string Host { get; init; } = default!;
    public string VirtualHost { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}
