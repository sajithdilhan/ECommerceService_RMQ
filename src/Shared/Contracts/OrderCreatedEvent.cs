namespace Shared.Contracts;

public class OrderCreatedEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}