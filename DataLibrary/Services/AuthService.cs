using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataLibrary.Context;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace DataLibrary.Services;

public interface IAuthService
{
    Task<Result<UserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken);
    Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Result> CreateRefreshTokenForUser(string userEmail, string token, CancellationToken cancellationToken);
    Task<Result<InternalUserAuthDto>> RotateRefreshTokenAsync(string currentToken, RefreshToken newRefreshToken, CancellationToken cancellationToken);
    Task<Result> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken);
}

public class AuthService : IAuthService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    private static string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return System.Convert.ToBase64String(hash).Replace('+','-').Replace('/','_').Replace("=", "");
    }

    public AuthService(SqliteContext context, IMapper mapper, ILogger<AuthService>? logger = null)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger ?? new LoggerFactory().CreateLogger<AuthService>();
    }

    public async Task<Result<UserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken)
    {
        try
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.Email == userDto.Email, cancellationToken);
            if (userExists is not null)
                return Result<UserAuthDto>.Failure("email taken.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            string salt = SharedLibrary.Helpers.SecurityHelpers.GenerateSalt();
            var hashedPasswordResult = SharedLibrary.Helpers.SecurityHelpers.HashPassword(userDto.Password, salt);
            if (!hashedPasswordResult.IsSuccess)
                return Result<UserAuthDto>.Failure(hashedPasswordResult.ErrorMessage!);

            var user = new User()
            {
                Email = userDto.Email,
                Username = userDto.Name,
                Height = userDto.Height ?? null,
                Gender = userDto.Gender ?? "U"
            };

            user.UserPasswords =
            [
                new UserPassword()
                {
                    IsCurrent = true,
                    CreatedAt = DateTime.Now,
                    PasswordHash = hashedPasswordResult.Value!,
                    PasswordSalt = salt,
                    User = user,
                },
            ];

            var userRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "user", cancellationToken);
            user.UserRoles = [new UserRole() { Role = userRole!, User = user }];

            if (userDto.Image is not null)
                user.UserProfileImages =
                [
                    new UserProfileImage()
                    {
                        Url = userDto.Image,
                        User = user,
                        IsPrimary = true,
                    }
                ];

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<UserAuthDto>.Success(new UserAuthDto()
            {
                Email = user.Email,
                Username = user.Email,
                Roles = user.UserRoles.Select(x => x.Role.Name).ToList(),
            }, "user created!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(CreateUserAsync));
            return Result<UserAuthDto>.Failure("something went wrong creating a user.", ex);
        }
    }

    public async Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .ProjectTo<InternalUserAuthDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (user is null)
                return Result<InternalUserAuthDto>.Failure("user not found");

            var latestPassword = await _context.Users
                .Include(x => x.UserPasswords.Where(x => x.IsCurrent == true))
                .FirstOrDefaultAsync(x => x.Email == user.Email, cancellationToken);

            user.LatestPasswordHash = latestPassword?.UserPasswords.FirstOrDefault()?.PasswordHash;
            return Result<InternalUserAuthDto>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(GetUserWithRolesByEmailAsync));
            return Result<InternalUserAuthDto>.Failure("something went wrong creating a user.", ex);
        }
    }

    public async Task<Result> CreateRefreshTokenForUser(string userEmail, string token, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var user = await _context.Users
                .Include(x => x.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == userEmail, cancellationToken);

            if (user is null) return Result.Failure("user does not exist.");

            foreach (var rToken in user.RefreshTokens)
            {
                rToken.Active = false;
            }

            user.RefreshTokens.Add(new RefreshToken()
            {
                Active = true,
                User = user,
                Token = HashToken(token),
                Expires = DateTime.UtcNow.AddDays(7)
            });

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(CreateRefreshTokenForUser));
            return Result.Failure("something went wrong creating a user.", ex);
        }
    }

    public async Task<Result<InternalUserAuthDto>> RotateRefreshTokenAsync(string currentToken, RefreshToken newRefreshToken, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var existingToken = await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == HashToken(currentToken), cancellationToken);

            if (existingToken is null || existingToken.Active != true || (existingToken.Expires.HasValue && existingToken.Expires <= DateTime.Now))
                return Result<InternalUserAuthDto>.Failure("invalid or expired refresh token");

            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken);

            if (user is null)
                return Result<InternalUserAuthDto>.Failure("user not found");

            foreach (var token in user.RefreshTokens)
            {
                token.Active = false;
                if (token.Token == existingToken.Token)
                    token.Revoked = DateTime.Now;
            }

            var storedNew = new RefreshToken
            {
                Active = true,
                User = user,
                Token = HashToken(newRefreshToken.Token),
                Expires = newRefreshToken.Expires
            };

            user.RefreshTokens.Add(storedNew);

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var mapped = _mapper.Map<InternalUserAuthDto>(user);
            return Result<InternalUserAuthDto>.Success(mapped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(RotateRefreshTokenAsync));
            return Result<InternalUserAuthDto>.Failure("something went wrong refreshing the token.", ex);
        }
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        try
        {
            var tokenHash = HashToken(token);
            var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
            if (existingToken is null)
                return Result.Failure("refresh token not found");
            if (existingToken.Active == false)
                return Result.Success();

            existingToken.Active = false;
            existingToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error in {Method}", nameof(RevokeRefreshTokenAsync));
            return Result.Failure("something went wrong revoking the token.", ex);
        }
    }
}

