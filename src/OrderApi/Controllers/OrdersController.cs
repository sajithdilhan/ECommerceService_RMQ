using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using OrderApi.Dtos;
using OrderApi.Services;
using Shared.Common;
using System.Net;
using System.Text.Json;

namespace OrderApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrdersService ordersService, ILogger<OrdersController> logger, IDistributedCache cache) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cts)
    {
        if (id == Guid.Empty)
        {
            logger.LogWarning("GetOrder called with an empty GUID.");
            return Problem("Invalid order ID.", statusCode: (int)HttpStatusCode.BadRequest);
        }

        var cacheKey = $"{Constants.CacheKeyOrderPrefix}{id}";
        var cached = await cache.GetStringAsync(cacheKey, cts);

        if (!string.IsNullOrWhiteSpace(cached))
        {
            return Ok(JsonSerializer.Deserialize<OrderResponse>(cached));
        }

        var result = await ordersService.GetOrderByIdAsync(id, cts);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result.Value), cts);

        return Ok(result.Value);
    }

    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(typeof(List<OrderResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetOrdersByUserId(Guid userId, CancellationToken cts)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("GetOrder called with an empty UserId GUID.");
            return Problem("Invalid user ID.", statusCode: (int)HttpStatusCode.BadRequest);
        }

        logger.LogInformation("Retrieving orders by user: {UserId}", userId);
        var order = await ordersService.GetOrdersByUserAsync(userId, cts);

        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateOrder(OrderCreationRequest? newOrder, CancellationToken cts)
    {
        if (IsInValidRequest(newOrder))
        {
            logger.LogWarning("CreateOrder called with invalid data.");
            return Problem("Invalid request data.", statusCode: (int)HttpStatusCode.BadRequest);
        }

        logger.LogInformation("Creating a new order: {@NewOrder}", JsonSerializer.Serialize(newOrder));
        var result = await ordersService.CreateOrderAsync(newOrder!, cts);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        await cache.SetStringAsync(
           $"{Constants.CacheKeyOrderPrefix}{result.Value!.Id}",
           JsonSerializer.Serialize(result.Value),
           new DistributedCacheEntryOptions
           {
               AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
           }, cts);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value.Id }, result.Value);
    }

    private static bool IsInValidRequest(OrderCreationRequest? newOrder)
    {
        return newOrder is null
            || string.IsNullOrWhiteSpace(newOrder?.Product)
            || newOrder.UserId == Guid.Empty
            || newOrder.Quantity <= 0
            || newOrder.Price < 0;
    }
}