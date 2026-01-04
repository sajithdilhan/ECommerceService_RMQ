using Shared.Models;

namespace OrderApi.Data;

public interface IOrderRepository 
{
    Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cts);
    Task<List<Order>> GetOrdersByUserAsync(Guid userId, CancellationToken cts);
    Task<Order?> CreateOrderAsync(Order newOrder, CancellationToken cts);
    Task<int> CreateKnownUserAsync(KnownUser knownUser, CancellationToken cts);
    Task<KnownUser?> GetKnownUserByIdAsync(Guid id, CancellationToken cts);
}