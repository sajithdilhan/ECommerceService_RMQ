using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace UserApi.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}