using API.Controllers;
using API.Security;

using DataLibrary.Models;
using DataLibrary.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace TrainingTests.APIControllersUnitTests;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _controller = new AuthController(_userServiceMock.Object, _tokenServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Register_ShouldReturnCreatedAtRouteResult_WhenRegistrationIsSuccessful()
    {
        var userWriteDto = new UserWriteDto
        {
            Name = "testuser",
            Email = "testuser@test.com",
            Password = "Password123!"
        };

        var createdUser = new InternalUserAuthDto
        {
            Username = "testuser",
            Email = "testuser@test.com",
            Roles = new List<string> { "user" }
        };

        _userServiceMock.Setup(s => s.CreateUserAsync(userWriteDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InternalUserAuthDto>.Success(createdUser));

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshToken { Token = "test_refresh_token", Expires = DateTime.UtcNow.AddDays(7) });

        _userServiceMock.Setup(s =>
                s.CreateRefreshTokenForUser(createdUser.Email, "test_refresh_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<InternalUserAuthDto>()))
            .Returns("dummy_jwt_token");

        var result = await _controller.Register(userWriteDto, CancellationToken.None);

        var actionResult = Assert.IsType<ActionResult<UserAuthDto>>(result);
        var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(actionResult.Result);
        var resultDto = Assert.IsType<UserAuthDto>(createdAtRouteResult.Value);

        Assert.Equal(createdUser.Username, resultDto.Username);
        Assert.Equal(createdUser.Email, resultDto.Email);
        Assert.Equal("dummy_jwt_token", resultDto.Token);

        _userServiceMock.Verify(s => s.CreateUserAsync(userWriteDto, It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
        _userServiceMock.Verify(
            s => s.CreateRefreshTokenForUser(createdUser.Email, "test_refresh_token", It.IsAny<CancellationToken>()),
            Times.Once);
        _tokenServiceMock.Verify(t => t.CreateToken(It.IsAny<InternalUserAuthDto>()), Times.Once);

        Assert.True(_controller.Response.Headers.ContainsKey("Set-Cookie"));
        var setCookieHeader = _controller.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("refreshToken=test_refresh_token", setCookieHeader);
    }

    [Fact]
    public async Task LogIn_ShouldReturnUserAuthDto_WhenLoginIsSuccessful()
    {
        var logInDto = new UserLogInDto
        {
            Email = "testuser@test.com",
            Password = "Password123!"
        };

        var userAuthDto = new InternalUserAuthDto
        {
            Username = "testuser",
            Email = "testuser@test.com",
            Roles = new List<string> { "user" },
            LatestPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") // Simulate the latest hash stored
        };

        _userServiceMock.Setup(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InternalUserAuthDto>.Success(userAuthDto));

        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshToken { Token = "test_refresh_token", Expires = DateTime.UtcNow.AddDays(7) });

        _userServiceMock.Setup(s => s.CreateRefreshTokenForUser(userAuthDto.Email, "test_refresh_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<InternalUserAuthDto>()))
            .Returns("dummy_jwt_token");  // Mocked to return a dummy token

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var actionResult = Assert.IsType<ActionResult<UserAuthDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var resultDto = Assert.IsType<UserAuthDto>(okResult.Value);

        Assert.Equal(userAuthDto.Username, resultDto.Username);
        Assert.Equal(userAuthDto.Email, resultDto.Email);

        // Ensure Token is set properly
        Assert.NotNull(resultDto.Token);
        Assert.Equal("dummy_jwt_token", resultDto.Token);

        _userServiceMock.Verify(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
        _userServiceMock.Verify(s => s.CreateRefreshTokenForUser(userAuthDto.Email, "test_refresh_token", It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(t => t.CreateToken(It.IsAny<InternalUserAuthDto>()), Times.Once);

        Assert.True(_controller.Response.Headers.ContainsKey("Set-Cookie"));
        var setCookieHeader = _controller.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("refreshToken=test_refresh_token", setCookieHeader);
    }


    [Fact]
    public async Task LogIn_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var logInDto = new UserLogInDto
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        };

        _userServiceMock.Setup(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InternalUserAuthDto>.Failure("User not found"));

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var actionResult = Assert.IsType<ActionResult<UserAuthDto>>(result);
        Assert.IsType<UnauthorizedResult>(actionResult.Result);

        _userServiceMock.Verify(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogIn_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        var logInDto = new UserLogInDto
        {
            Email = "testuser@test.com",
            Password = "WrongPassword"
        };

        var userAuthDto = new InternalUserAuthDto
        {
            Username = "testuser",
            Email = "testuser@test.com",
            Roles = new List<string> { "user" }
        };

        _userServiceMock.Setup(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InternalUserAuthDto>.Success(userAuthDto));

        _userServiceMock.Setup(s => s.CreateRefreshTokenForUser(userAuthDto.Email, "test_refresh_token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Password verification failed"));

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var actionResult = Assert.IsType<ActionResult<UserAuthDto>>(result);
        Assert.IsType<UnauthorizedResult>(actionResult.Result);

        _userServiceMock.Verify(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userServiceMock.Verify(s => s.CreateRefreshTokenForUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogIn_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        var logInDto = new UserLogInDto
        {
            Email = "testuser@test.com",
            Password = "Password123!"
        };

        _userServiceMock.Setup(s => s.GetUserWithRolesByEmailAsync(logInDto.Email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        var result = await _controller.LogIn(logInDto, CancellationToken.None);

        var actionResult = Assert.IsType<ActionResult<UserAuthDto>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An unexpected error occurred", objectResult.Value);
    }
}