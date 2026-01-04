
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace UserApi.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User?> CreateUserAsync(User newUser, CancellationToken cts)
        {
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cts)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => string.Equals(u.Email, email, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cts)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}