using OrderApi.Dtos;
using Shared.Models;

namespace OrderApi.Services;

public interface IOrdersService
{
    public Task<Result<OrderResponse>> CreateOrderAsync(OrderCreationRequest newOrder, CancellationToken cts);
    public Task<Result<OrderResponse>> GetOrderByIdAsync(Guid id, CancellationToken cts);
    public Task<Result<List<OrderResponse>>> GetOrdersByUserAsync(Guid userId, CancellationToken cts);
    public Task CreateKnownUserAsync(KnownUser knownUser, CancellationToken cts);
}
