using Shared.Models;
using UserApi.Dtos;

namespace UserApi.Services;

public interface IUsersService
{
    public Task<Result<UserResponse>> CreateUserAsync(UserCreationRequest newUser, CancellationToken cts);
    public Task<Result<UserResponse>> GetUserByIdAsync(Guid id, CancellationToken cts);
}
