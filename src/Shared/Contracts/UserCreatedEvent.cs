namespace Shared.Contracts;

public record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}