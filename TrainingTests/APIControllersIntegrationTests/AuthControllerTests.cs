using System.Security.Cryptography;
using API.Controllers;
using API.Security;
using DataLibrary.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Dtos;
using TrainingTests.helpers;

namespace TrainingTests.APIControllersIntegrationTests;

public class AuthControllerIntegrationTests : BaseTestClass
{
    private readonly AuthController _controller;
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private Mock<ILogger<UserService>> _userServiceLoggerMock;
    private Mock<IConfiguration> _configurationMock;

    public AuthControllerIntegrationTests()
    {
        _userServiceLoggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_context, _mapper, _userServiceLoggerMock.Object);
        var secureKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["TokenKey"]).Returns(secureKey);

        _tokenService = new TokenService(_configurationMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton<IUserService>(_userService);
        services.AddSingleton<ITokenService>(_tokenService);
        services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

        var serviceProvider = services.BuildServiceProvider();

        _controller = new AuthController(_userService, _tokenService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
            }
        };
    }
    
    [Fact]
    public async Task Register_ShouldCreateUserAndSetRefreshToken_WhenRegistrationIsSuccessful()
    {
        ProductionDatabaseHelpers.SeedRoles(_context); 

        var newUser = new UserWriteDto
        {
            Email = "legolas@test.com",
            Password = "password123",
            Name = "Legolas",
            Gender = "M"
        };

        var result = await _controller.Register(newUser, CancellationToken.None);

        var createdAtResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.NotNull(createdAtResult);
        Assert.Equal(nameof(_controller.Register), createdAtResult.RouteName);

        var createdUserEmail = newUser.Email;

        var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createdUserEmail);
        Assert.NotNull(createdUser);

        var refreshToken = _context.RefreshTokens
            .FirstOrDefault(rt => rt.UserId == createdUser.Id && rt.Active.HasValue && rt.Active.Value);
        Assert.NotNull(refreshToken);
    }
    
    [Fact]
    public async Task Register_ShouldFail_WhenUserAlreadyExists()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var existingUser = new UserWriteDto
        {
            Email = "canaan@test.com",
            Password = "password123",
            Name = "Canaan",
            Gender = "M"
        };

        var result = await _controller.Register(existingUser, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult);
        Assert.Equal("email taken.", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenInvalidDataIsProvided()
    {
        var invalidUser = new UserWriteDto
        {
            Email = "",  // Invalid email
            Password = "pass",
            Name = "",
            Gender = "Unknown"
        };

        var result = await _controller.Register(invalidUser, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult);
    }

    [Fact]
    public async Task LogIn_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        ProductionDatabaseHelpers.SeedRoles(_context);

        var logInDto = new UserLogInDto
        {
            Email = "canaan@test.com",
            Password = "wrongpassword"
        };

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.NotNull(unauthorizedResult);
    }

    [Fact]
    public async Task LogIn_ShouldFail_WhenUserDoesNotExist()
    {
        var logInDto = new UserLogInDto
        {
            Email = "nonexistent@test.com",
            Password = "somepassword"
        };

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.NotNull(unauthorizedResult);
    }

    [Fact]
    public async Task LogIn_ShouldReturnUserAuthDtoAndSetRefreshToken_WhenLoginIsSuccessful()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var logInDto = new UserLogInDto
        {
            Email = "canaan@test.com",
            Password = "كنعان لازم يتدرب !"
        };

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult);

        var userDto = Assert.IsType<UserAuthDto>(okResult.Value);
        Assert.Equal(logInDto.Email, userDto.Email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        Assert.NotNull(user);

        var refreshToken = _context.RefreshTokens
            .FirstOrDefault(rt => rt.UserId == user.Id && rt.Active.HasValue && rt.Active.Value);
        Assert.NotNull(refreshToken);
    }

}
