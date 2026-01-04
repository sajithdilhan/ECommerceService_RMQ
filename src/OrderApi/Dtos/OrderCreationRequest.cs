using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderApi.Dtos;

public class OrderCreationRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public required string Product { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }

    public Order MapToOrder()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = this.UserId,
            Product = this.Product,
            Quantity = this.Quantity,
            Price = this.Price
        };
    }
}