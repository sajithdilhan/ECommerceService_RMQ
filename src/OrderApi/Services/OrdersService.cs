using OrderApi.Data;
using OrderApi.Dtos;
using Shared.Contracts;
using Shared.Models;
using System.Net;

namespace OrderApi.Services;

public class OrdersService(IOrderRepository orderRepository, ILogger<OrdersService> logger, IKafkaProducerWrapper producer) : IOrdersService
{
    public async Task<Result<OrderResponse>> CreateOrderAsync(OrderCreationRequest newOrder, CancellationToken cts)
    {
        try
        {
            var order = newOrder.MapToOrder();
            bool isValidUser = await ValidateUser(order, cts);
            if (!isValidUser)
            {
                logger.LogError("Attempted to create order for unknown user ID {UserId}", order.UserId);
                return Result<OrderResponse>.Failure(new Error((int)HttpStatusCode.BadRequest, $"Attempted to create order for unknown user ID {order.UserId}"));
            }

            var createdOrder = await orderRepository.CreateOrderAsync(order, cts);

            if (createdOrder == null)
            {
                logger.LogError("Failed to create order for user ID {UserId}", order.UserId);
                return Result<OrderResponse>.Failure(new Error((int)HttpStatusCode.InternalServerError, $"Order creation failed."));
            }

            await producer.ProduceAsync(createdOrder.Id,
                    new OrderCreatedEvent
                    {
                        Id = createdOrder.Id,
                        UserId = createdOrder.UserId,
                        Price = createdOrder.Price,
                        Product = createdOrder.Product,
                        Quantity = createdOrder.Quantity
                    }, cts);

            return Result<OrderResponse>.Success(OrderResponse.MapOrderToResponseDto(createdOrder));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating order");
            return Result<OrderResponse>.Failure(new Error((int)HttpStatusCode.InternalServerError, $"Order creation failed."));
        }
    }

    public async Task<Result<OrderResponse>> GetOrderByIdAsync(Guid id, CancellationToken cts)
    {
        var order = await orderRepository.GetOrderByIdAsync(id, cts);

        if (order is null)
        {
            return Result<OrderResponse>.Failure(new Error((int)HttpStatusCode.NotFound, $"Order with ID {id} not found."));
        }

        return Result<OrderResponse>.Success(OrderResponse.MapOrderToResponseDto(order));
    }

    public async Task<Result<List<OrderResponse>>> GetOrdersByUserAsync(Guid userId, CancellationToken cts)
    {
        var orders = await orderRepository.GetOrdersByUserAsync(userId, cts);

        if (orders.Count == 0)
        {
            return Result<List<OrderResponse>>.Failure(new Error((int)HttpStatusCode.NotFound, $"Order with UserId {userId} not found."));
        }

        return Result<List<OrderResponse>>.Success([.. orders.Select(o => OrderResponse.MapOrderToResponseDto(o))]);
    }

    public async Task CreateKnownUserAsync(KnownUser knownUser, CancellationToken cts)
    {
        try
        {
            var existingUser = await orderRepository.GetKnownUserByIdAsync(knownUser.UserId, cts);
            if (existingUser == null)
            {
                logger.LogInformation("Creating new known user with ID {UserId}", knownUser.UserId);
                await orderRepository.CreateKnownUserAsync(knownUser, cts);
            }
        }
        catch (Exception)
        {
            logger.LogError("Error occurred while creating known user with ID {UserId}", knownUser.UserId);
            throw;
        }
    }

    private async Task<bool> ValidateUser(Order order, CancellationToken cts)
    {
        var existingUser = await orderRepository.GetKnownUserByIdAsync(order.UserId, cts);
        return existingUser != null;
    }
}