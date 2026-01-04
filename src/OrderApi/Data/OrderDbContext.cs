using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace OrderApi.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<KnownUser> KnownUsers { get; set; }
}