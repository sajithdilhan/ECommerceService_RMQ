using Shared.Contracts;
using Shared.Models;
using System.Net;
using UserApi.Data;
using UserApi.Dtos;

namespace UserApi.Services;

public class UsersService(IUserRepository userRepository, ILogger<UsersService> logger) : IUsersService
{
    public async Task<Result<UserResponse>> CreateUserAsync(UserCreationRequest newUser, CancellationToken cts)
    {
        try
        {
            var user = newUser.MapToUser();

            var exsistingUser = await userRepository.GetUserByEmailAsync(user.Email, cts);
            if (exsistingUser != null)
            {
                logger.LogWarning("Conflict occurred while creating user with Email: {UserEmail}", newUser.Email);
                return Result<UserResponse>.Failure(new Error((int)HttpStatusCode.Conflict, $"User with email {user.Email} already exists."));
            }

            var createdUser = await userRepository.CreateUserAsync(user, cts);
            if (createdUser == null)
            {
                logger.LogError("Repository failed to create user for: {UserEmail}", newUser.Email);
                return Result<UserResponse>.Failure(new Error((int)HttpStatusCode.InternalServerError,
                    $"Repository failed to create user for: {newUser.Email}"));
            }

            // Produce user created event to message bus (omitted for brevity)

            return Result<UserResponse>.Success(UserResponse.MapUserToResponseDto(createdUser));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating user with email:{Email}", newUser.Email);
            return Result<UserResponse>.Failure(new Error((int)HttpStatusCode.InternalServerError,
                $"An error occurred while creating user with email:{newUser.Email}"));
        }
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(Guid id, CancellationToken cts)
    {
        try
        {
            var response = await userRepository.GetUserByIdAsync(id, cts);

            if (response == null)
            {
                logger.LogWarning("User with ID: {UserId} not found.", id);
                return Result<UserResponse>.Failure(new Error((int)HttpStatusCode.NotFound, $"User with ID {id} not found."));
            }

            return Result<UserResponse>.Success(UserResponse.MapUserToResponseDto(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving user with ID: {UserId}", id);
            return Result<UserResponse>.Failure(new Error((int)HttpStatusCode.InternalServerError,
                $"An error occurred while retrieving user with ID: {id}"));
        }
    }
}