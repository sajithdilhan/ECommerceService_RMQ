using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Common;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using UserApi.Dtos;
using UserApi.Services;

namespace UserApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IUsersService usersService, ILogger<UsersController> logger, IDistributedCache cache) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cts)
    {
        if (id == Guid.Empty)
        {
            logger.LogWarning("GetUser called with an empty GUID.");
            return Problem("Invalid user ID.", statusCode: (int)HttpStatusCode.BadRequest);
        }

        logger.LogInformation("Retrieving user with ID: {UserId}", id);

        var cached = await cache.GetStringAsync($"{Constants.CacheKeyUserPrefix}{id}", cts);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            logger.LogInformation("User with ID: {UserId} found in cache.", id);
            return Ok(JsonSerializer.Deserialize<UserResponse>(cached));
        }

        var result = await usersService.GetUserByIdAsync(id, cts);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        await cache.SetStringAsync(
            $"{Constants.CacheKeyUserPrefix}{id}",
            JsonSerializer.Serialize(result.Value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateUser(UserCreationRequest? newUser, CancellationToken cts)
    {
        if (IsInValidRequest(newUser))
        {
            logger.LogWarning("CreateUser called with invalid data.");
            return Problem("Invalid request data.", statusCode: (int)HttpStatusCode.BadRequest);
        }

        logger.LogInformation("Creating a new user {@user}", JsonSerializer.Serialize(newUser));
        var result = await usersService.CreateUserAsync(newUser!, cts);

        if (!result.IsSuccess)
        {
            Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        await cache.SetStringAsync(
            $"{Constants.CacheKeyUserPrefix}{result.Value!.Id}",
            JsonSerializer.Serialize(result.Value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }, cts);

        return CreatedAtAction(nameof(GetUser), new { id = result.Value.Id }, result.Value);
    }

    private static bool IsInValidRequest(UserCreationRequest? newUser)
    {
        return newUser is null
            || string.IsNullOrWhiteSpace(newUser?.Name)
            || string.IsNullOrWhiteSpace(newUser?.Email)
            || !IsValidEmailFormat(newUser?.Email);
    }

    private static bool IsValidEmailFormat(string? email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}