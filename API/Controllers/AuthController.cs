using API.Common.Validators;
using API.Security;
using DataLibrary.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(
        IUserService userService,
        ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    // register
    [AllowAnonymous]
    [HttpPost("/register")]
    [SwaggerOperation(Summary = "Normal version", Description = "ROG IN !")]
    public async Task<ActionResult<UserAuthDto>> Register(
        UserWriteDto userWriteDto, CancellationToken cancellationToken)
    {
        try
        {
            var registerationResult = await _userService.CreateUserAsync(userWriteDto, cancellationToken);
            if (!registerationResult.IsSuccess)
                return BadRequest(registerationResult.ErrorMessage);

            var user = registerationResult.Value;
            await SetRefreshToken(user.Email, cancellationToken);

            var internalUser = new InternalUserAuthDto
            {
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles ?? new List<string>()
            };
            var response = CreateUserObject(internalUser);
            // NOTE: return 200 rather than CreatedAtRoute — there is no named "Register" route
            // nor a GET-by-id endpoint, so CreatedAtRoute throws "No route matches the supplied
            // values." during result execution and turns a successful registration into a 500.
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private async Task SetRefreshToken(string userEmail, CancellationToken cancellationToken)
    {
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _userService.CreateRefreshTokenForUser(userEmail, refreshToken.Token, cancellationToken);

        var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Path = "/",
    Expires = refreshToken.Expires.HasValue ? new DateTimeOffset(refreshToken.Expires.Value) : DateTimeOffset.UtcNow.AddDays(7),
};

        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
    }

    private UserAuthDto CreateUserObject(InternalUserAuthDto internalUserDto)
    {
        return new UserAuthDto
        {
            Email = internalUserDto.Email,
            Token = _tokenService.CreateToken(
                new InternalUserAuthDto
                {
                    Username = internalUserDto.Username,
                    Email = internalUserDto.Email,
                    Roles = internalUserDto.Roles,
                    Id = internalUserDto.Id,
                }),
            Roles = internalUserDto.Roles,
            Username = internalUserDto.Username
        };
    }

    [AllowAnonymous]
    [HttpPost("/log-in")]
    [EnableRateLimiting("strict-login")]
    public async Task<ActionResult<UserAuthDto>> LogIn(
        [FromBody] UserLogInDto logInDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userResult = await _userService.GetUserWithRolesByEmailAsync(logInDto.Email, cancellationToken);
            if (!userResult.IsSuccess)
                return Unauthorized();

            var user = userResult.Value;
            var verificationResult = SecurityHelpers.VerifyPassword(logInDto.Password, user!.LatestPasswordHash);
            if (!verificationResult.IsSuccess)
                return Unauthorized();

            await SetRefreshToken(user.Email, cancellationToken);
            user.LatestPasswordHash = null;

            return Ok(CreateUserObject(user));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    [AllowAnonymous]
    [HttpPost("/refresh")]
    // Not rate-limited with "strict-login": refresh tokens are 64-byte random values (not
    // guessable like a password), and this policy (5/15min per IP) would lock out legitimate
    // automatic refreshes. Add a dedicated, looser policy here if throttling is wanted.
    public async Task<ActionResult<UserAuthDto>> Refresh(CancellationToken cancellationToken)
    {
        try
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var currentToken) || string.IsNullOrWhiteSpace(currentToken))
                return Unauthorized();

            var newRefresh = _tokenService.GenerateRefreshToken();
            var rotation = await _userService.RotateRefreshTokenAsync(currentToken, newRefresh, cancellationToken);
            if (!rotation.IsSuccess)
                return Unauthorized();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newRefresh.Expires.HasValue ? new DateTimeOffset(newRefresh.Expires.Value) : DateTimeOffset.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", newRefresh.Token, cookieOptions);

            var response = CreateUserObject(rotation.Value);
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    [AllowAnonymous]
    [HttpPost("/logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var token) && !string.IsNullOrWhiteSpace(token))
        {
            await _userService.RevokeRefreshTokenAsync(token, cancellationToken);
        }
        var deleteOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
        };
        Response.Cookies.Delete("refreshToken", deleteOptions);
        return NoContent();
    }}


