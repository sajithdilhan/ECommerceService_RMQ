namespace Shared.Contracts;

public record OrderCreatedEvent
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public string Product { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal Price { get; init; }
}