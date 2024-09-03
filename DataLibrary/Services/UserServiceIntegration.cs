using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataLibrary.Context;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Core;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;

namespace DataLibrary.Services;

public interface IUserService
{
    Task<Result<InternalUserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken);

    Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email,
        CancellationToken cancellationToken);

    Task<Result> CreateRefreshTokenForUser(string userEmail, string token,
        CancellationToken cancellationToken);
}

public class UserService : IUserService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(SqliteContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<InternalUserAuthDto>> CreateUserAsync(UserWriteDto userDto, CancellationToken cancellationToken)
    {
        try
        {
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.Email == userDto.Email,
                cancellationToken: cancellationToken);
            if (userExists is not null)
                return Result<InternalUserAuthDto>.Failure("email taken.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var salt = SecurityHelpers.GenerateSalt();
            var hashedPasswordResult = SecurityHelpers.HashPassword(userDto.Password, salt);
            if (!hashedPasswordResult.IsSuccess)
                return Result<InternalUserAuthDto>.Failure(hashedPasswordResult.ErrorMessage!);
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
            var userRole =
                await _context.Roles.FirstOrDefaultAsync(x => x.Name == "user", cancellationToken: cancellationToken);

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

            return Result<InternalUserAuthDto>.Success(new InternalUserAuthDto()
            {
                Email = user.Email,
                Username = user.Email,
                Roles = user.UserRoles.Select(x => x.Role.Name).ToList(),
                
            }, "user created!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"an error occu'd creating a user in {nameof(CreateUserAsync)}\nex:\t{ex}");
            return Result<InternalUserAuthDto>.Failure("something went wrong creating a user.", ex);
        }
    }

    public async Task<Result<InternalUserAuthDto>> GetUserWithRolesByEmailAsync(string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .ProjectTo<InternalUserAuthDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken: cancellationToken);

            var latestPassword = await _context.Users
                .Include(x => x.UserPasswords
                    .Where(x => x.IsCurrent == true))
                .FirstOrDefaultAsync(x => x.Email == user.Email, cancellationToken);
            
            user.LatestPasswordHash = latestPassword.UserPasswords.First().PasswordHash;
            
            return user is null ? Result<InternalUserAuthDto>.Failure("user not found") : Result<InternalUserAuthDto>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"an error occu'd creating a user in {nameof(GetUserWithRolesByEmailAsync)}\nex:\t{ex}");
            return Result<InternalUserAuthDto>.Failure("something went wrong creating a user.", ex);
        }
    }

    public async Task<Result> CreateRefreshTokenForUser(string userEmail, string token,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
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
                Token = token,
                Expires = DateTime.Now.AddHours(7) // TODO: in the config
            });

            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"an error occu'd creating a user in {nameof(CreateRefreshTokenForUser)}\nex:\t{ex}");
            return Result.Failure("something went wrong creating a user.", ex);
        }
    }

    // user plans 
    // get active plan
    // set active plan
    // (set/adjust) active plan start/end date
    // (finish/start) plan
    // user profile
    // user password
    // create 
    // update
    // change
    // user images (blob or disk ?)
    // CRUD
    // set primary
    // user muscles 
    // cool down calculation
    // frequency 
    // user exercises 
}