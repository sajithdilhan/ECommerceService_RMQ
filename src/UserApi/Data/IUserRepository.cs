using Shared.Models;

namespace UserApi.Data
{
    public interface IUserRepository 
    {
        Task<User?> GetUserByIdAsync(Guid id, CancellationToken cts);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cts);
        Task<User?> CreateUserAsync(User newUser, CancellationToken cts);
    }
}
