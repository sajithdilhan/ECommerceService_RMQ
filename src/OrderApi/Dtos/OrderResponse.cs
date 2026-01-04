using Shared.Models;

namespace OrderApi.Dtos;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }


    public static OrderResponse MapOrderToResponseDto(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Product = order.Product,
            Quantity = order.Quantity,
            Price = order.Price

        };
    }
}