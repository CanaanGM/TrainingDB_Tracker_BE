﻿using API.Common.Validators;
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
        IUserService userService
        , ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }


    // register
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
            return CreatedAtRoute(nameof(Register), CreateUserObject(user));
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
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
    }

    private UserAuthDto CreateUserObject(InternalUserAuthDto internalUserDto)
    {

        return new UserAuthDto
        {
            Email = internalUserDto.Email,
            Token = _tokenService.CreateToken(
            new InternalUserAuthDto()
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

    [HttpPost("/log-in")]
    [EnableRateLimiting("strict-login")]
    public async Task<ActionResult<UserAuthDto>> LogIn(
        [FromBody]  UserLogInDto logInDto
        , CancellationToken cancellationToken)
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
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }


}