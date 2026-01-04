using Microsoft.Extensions.Logging;
using Moq;
using Shared.Models;
using UserApi.Data;
using UserApi.Dtos;
using UserApi.Services;

namespace ECommerceTests.UserApiTests;

public class UsersServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<ILogger<UsersService>> _logger;
    private readonly CancellationToken _cts = new CancellationToken();

    public UsersServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<UsersService>>();
    }

    [Fact]
    public async Task GetUserByIdAsync_ReurnsUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>(), _cts))
            .ReturnsAsync(new User
            {
                Id = userId,
                Name = "Test User",
                Email = ""
            }
            );

        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act
        var result = await usersService.GetUserByIdAsync(userId, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value?.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userRepository.Setup(repo => repo.GetUserByIdAsync(userId, _cts)).ReturnsAsync((User?)null);
        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act
        var result = await usersService.GetUserByIdAsync(userId, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal($"User with ID {userId} not found.", result.Error?.Message);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsFailure_WhenDb_Exception()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userRepository.Setup(repo => repo.GetUserByIdAsync(userId, _cts)).ThrowsAsync(new Exception("Database error"));

        var usersService = new UsersService(_userRepository.Object, _logger.Object);


        // Act
        var result = await usersService.GetUserByIdAsync(userId, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal($"An error occurred while retrieving user with ID: {userId}", result.Error?.Message);
        _userRepository.Verify(r => r.GetUserByIdAsync(It.IsAny<Guid>(), _cts), Times.Once);

    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCreatedUser_WhenValidRequest()
    {
        // Arrange
        var newUserRequest = new UserCreationRequest
        {
            Name = "New User",
            Email = "sajith@mail.com"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Name = newUserRequest.Name,
            Email = newUserRequest.Email
        };

        _userRepository.Setup(repo => repo.GetUserByEmailAsync(newUserRequest.Email, _cts))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), _cts))
            .ReturnsAsync(createdUser);

        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act
        var result = await usersService.CreateUserAsync(newUserRequest, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdUser.Id, result.Value?.Id);
        Assert.Equal(createdUser.Name, result.Value?.Name);
        Assert.Equal(createdUser.Email, result.Value?.Email);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsConflict_WhenEmailExists()
    {
        // Arrange
        var newUserRequest = new UserCreationRequest
        {
            Name = "New User",
            Email = "sajith@mail.com"
        };

        _userRepository.Setup(repo => repo.GetUserByEmailAsync(newUserRequest.Email, _cts))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Name = "Existing User",
                Email = newUserRequest.Email
            });

        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act
        var result = await usersService.CreateUserAsync(newUserRequest, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains($"User with email {newUserRequest.Email} already exists.", result.Error?.Message);
        _userRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>(), _cts), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsFailure_WhenRepositoryReturnsNull()
    {
        // Arrange
        var newUserRequest = new UserCreationRequest
        {
            Name = "New User",
            Email = "sajith@mail.com"
        };

        _userRepository.Setup(repo => repo.GetUserByEmailAsync(newUserRequest.Email, _cts))
            .ReturnsAsync((User?)null);
        _userRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), _cts))
            .ReturnsAsync((User?)null);

        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act 
        var result = await usersService.CreateUserAsync(newUserRequest, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal($"Repository failed to create user for: {newUserRequest.Email}", result.Error?.Message);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        var newUserRequest = new UserCreationRequest
        {
            Name = "New User",
            Email = "sajith@mail.com"
        };

        _userRepository.Setup(repo => repo.GetUserByEmailAsync(newUserRequest.Email, _cts))
            .ThrowsAsync(new Exception("Database connection failed"));

        var usersService = new UsersService(_userRepository.Object, _logger.Object);

        // Act
        var result = await usersService.CreateUserAsync(newUserRequest, _cts);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal($"An error occurred while creating user with email:{newUserRequest.Email}", result.Error?.Message);
        _userRepository.Verify(r => r.CreateUserAsync(It.IsAny<User>(), _cts), Times.Never);
    }
}