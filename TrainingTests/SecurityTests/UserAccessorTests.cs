using System.Security.Claims;
using API.Security;
using Microsoft.AspNetCore.Http;
using Moq;

namespace TrainingTests.SecurityTests;

public class UserAccessorTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserAccessor _userAccessor;

    public UserAccessorTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _userAccessor = new UserAccessor(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void GetUsername_ShouldReturnCorrectUsername_WhenUserIsAuthenticated()
    {
        var username = "testuser";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(principal);

        var result = _userAccessor.GetUsername();
        Assert.True(result.IsSuccess);
        Assert.Equal(username, result.Value);
    }

    [Fact]
    public void GetUsername_ShouldReturnNull_WhenUserIsNotAuthenticated()
    {
        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(new ClaimsPrincipal());

        var result = _userAccessor.GetUsername();
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("user is not authenticated.", result.ErrorMessage);
    }

    [Fact]
    public void GetUserId_ShouldReturnCorrectUserId_WhenUserIsAuthenticated()
    {
        var userId = 123;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(principal);

        var result = _userAccessor.GetUserId();

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value);
    }

    [Fact]
    public void GetUserId_ShouldReturnZero_WhenUserIdClaimIsNotPresent()
    {
        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(new ClaimsPrincipal());

        var result = _userAccessor.GetUserId();
        Assert.False(result.IsSuccess);
        Assert.Equal("user is not authenticated.", result.ErrorMessage);
    }
}