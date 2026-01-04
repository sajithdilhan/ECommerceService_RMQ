using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace OrderApi.Data;

public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    public async Task<int> CreateKnownUserAsync(KnownUser knownUser, CancellationToken cts)
    {
        await context.KnownUsers.AddAsync(knownUser);
        return await context.SaveChangesAsync();
    }

    public async Task<Order?> CreateOrderAsync(Order newOrder, CancellationToken cts)
    {
        await context.Orders.AddAsync(newOrder);
        await context.SaveChangesAsync();
        return newOrder;
    }

    public async Task<KnownUser?> GetKnownUserByIdAsync(Guid id, CancellationToken cts)
    {
        return await context.KnownUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cts)
    {
        return await context.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<Order>> GetOrdersByUserAsync(Guid userId, CancellationToken cts)
    {
        return context.Orders.AsNoTracking()
             .Where(o => o.UserId == userId)
             .Select(o => new Order
             {
                 Id = o.Id,
                 UserId = o.UserId,
                 Quantity = o.Quantity,
                 Price = o.Price,
                 Product = o.Product
             })
             .ToListAsync();
    }
}