using API.Security;
using DataLibrary.Models;
using DataLibrary.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dtos;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;

    public AuthController(IUserService userService, TokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }
    
    
    // register
    [HttpPost("register")]
    public async Task<ActionResult<UserAuthDto>> Register(UserWriteDto userWriteDto, CancellationToken cancellationToken)
    {
        try
        {
            var registerationResult = await _userService.CreateUserAsync(userWriteDto, cancellationToken);
            if (!registerationResult.IsSuccess)
                return BadRequest( registerationResult.ErrorMessage);
            
            var user = registerationResult.Value;
            await SetRefreshToken(user.Email,cancellationToken);
            return CreateUserObject(user); 
        }
        catch (Exception e)
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
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
    }

    private UserAuthDto CreateUserObject(UserAuthDto userDto)
    {
        userDto.Token = _tokenService.CreateToken(
            new UserAuthDto()
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Roles = userDto.Roles,

            });
        return userDto;
    } 
    // log in
}