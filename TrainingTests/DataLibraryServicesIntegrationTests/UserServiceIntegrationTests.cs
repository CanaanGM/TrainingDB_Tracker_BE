using DataLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Dtos;
using TrainingTests.helpers;

namespace TrainingTests.ServicesTests;

public class UserServiceIntegrationTests : BaseIntegrationTestClass
{
    private Mock<ILogger<UserService>> _logger;
    private readonly UserService _serviceIntegration;
    
    public UserServiceIntegrationTests()
    {
        _logger = new Mock<ILogger<UserService>>();
        _serviceIntegration = new UserService(_context, _mapper, _logger.Object);
    }

    
    [Fact]
    public async Task CreateUserSuccess()
    {
        ProductionDatabaseHelpers.SeedRoles(_context);
        var newUser = new UserWriteDto()
        {
            Email = "legolas@test.com",
            Password = "they're taking the hobbits to isenguard!",
            Name = "legolas",
            Gender = "M"
        };

        var result = await _serviceIntegration.CreateUserAsync(newUser, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Equal("user created!", result.SuccessMessage);

        var createdUser = _context.Users
            .Include(x => x.UserPasswords)
            .Include(x => x.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault();
        Assert.NotNull(createdUser);
        
        Assert.True(BCrypt.Net.BCrypt.Verify(newUser.Password, createdUser.UserPasswords.First().PasswordHash));
        Assert.Single(createdUser.UserRoles);
        Assert.Equal("user", createdUser.UserRoles.First().Role.Name);
        Assert.True(createdUser.Id >= 1);
    }

    [Fact]
    public async Task CreateUserSuccess_SimilarUserName()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newUser = new UserWriteDto()
        {
            Email = "canaan1@test.com",
            Password = "they're taking the hobbits to isenguard!",
            Name = "Canaan",
            Gender = "F"
        };
        
        var result = await _serviceIntegration.CreateUserAsync(newUser, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Equal("user created!", result.SuccessMessage);

        var createdUser = _context.Users
            .Include(x => x.UserPasswords)
            .Include(x => x.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(z => z.Email == newUser.Email);
        
        Assert.NotNull(createdUser);
        var userPassword = createdUser.UserPasswords.First().PasswordHash;
        Assert.True(BCrypt.Net.BCrypt.Verify(newUser.Password,userPassword ));
        Assert.Single(createdUser.UserRoles);
        Assert.Equal("user", createdUser.UserRoles.First().Role.Name);
        Assert.True(createdUser.Id >= 1);
    }
    
    [Fact]
    public async Task CreateUserFail_SimilarEmail()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newUser = new UserWriteDto()
        {
            Email = "canaan@test.com",
            Password = "they're taking the hobbits to isenguard!",
            Name = "Canaan",
            Gender = "F"
        };
        
        var result = await _serviceIntegration.CreateUserAsync(newUser, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal("email taken.", result.ErrorMessage);

    }
    
    [Fact]
    public async Task CreateUserSuccess_NoGender()
    {
        ProductionDatabaseHelpers.SeedRoles(_context);
        var newUser = new UserWriteDto()
        {
            Email = "canaan@test.com",
            Password = "they're taking the hobbits to isenguard!",
            Name = "Canaan"
        };
        
        var result = await _serviceIntegration.CreateUserAsync(newUser, new CancellationToken());
        Assert.True(result.IsSuccess);
        
        var createdUser = _context.Users
            .Include(x => x.UserPasswords)
            .Include(x => x.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(z => z.Email == newUser.Email);
        
        Assert.NotNull(createdUser);
        var userPassword = createdUser.UserPasswords.First().PasswordHash;
        Assert.True(BCrypt.Net.BCrypt.Verify(newUser.Password,userPassword ));
        Assert.Single(createdUser.UserRoles);
        Assert.Equal("user", createdUser.UserRoles.First().Role.Name);
        Assert.Equal("U", createdUser.Gender);
        Assert.True(createdUser.Id >= 1);
    }
    
    [Fact]
    public async Task CreateUserSuccess_WithImage()
    {
        ProductionDatabaseHelpers.SeedRoles(_context);
        var newUser = new UserWriteDto()
        {
            Email = "canaan@test.com",
            Password = "they're taking the hobbits to isenguard!",
            Name = "Canaan",
            Image = "http://image_goes_here!"
        };
        
        var result = await _serviceIntegration.CreateUserAsync(newUser, new CancellationToken());
        Assert.True(result.IsSuccess);
        
        var createdUser = _context.Users
            .Include(x => x.UserPasswords)
            .Include(x => x.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(user => user.UserProfileImages)
            .FirstOrDefault(z => z.Email == newUser.Email);
        
        Assert.NotNull(createdUser);
        var userPassword = createdUser.UserPasswords.First().PasswordHash;
        Assert.True(BCrypt.Net.BCrypt.Verify(newUser.Password,userPassword ));
        Assert.Single(createdUser.UserRoles);
        Assert.Equal("user", createdUser.UserRoles.First().Role.Name);
        Assert.Equal("http://image_goes_here!", createdUser.UserProfileImages.First().Url);
        Assert.Equal(newUser.Email, result.Value.Email);
        Assert.Equal(createdUser.Email, result.Value.Email);
        Assert.True(createdUser.Id >= 1);
    }

    [Fact]
    public async Task GetUserWithRolesByEmail_Success()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var result = await _serviceIntegration.GetUserWithRolesByEmailAsync("canaan@test.com", new CancellationToken());
        
        Assert.True(result.IsSuccess);

        var userAuthDto = result.Value;
        Assert.True(userAuthDto.Roles.Count >= 1);
        Assert.Equal("Canaan", userAuthDto.Username);
        Assert.True(userAuthDto.Id >= 1);
        Assert.NotNull(userAuthDto.LatestPasswordHash);

    }
    
    [Fact]
    public async Task CreateRefreshTokenForAlphrad_ShouldDeactivateOldTokens()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var result = await _serviceIntegration.CreateRefreshTokenForUser("alphrad@test.com", "new_token_for_alphrad", CancellationToken.None);
        Assert.True(result.IsSuccess);

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == "alphrad@test.com");

        Assert.NotNull(user);
        Assert.Single(user.RefreshTokens.Where(x => x.Active!.Value && x.Token == "new_token_for_alphrad"));
        Assert.Equal(4, user.RefreshTokens.Count);
        Assert.All(user.RefreshTokens.Where(x => x.Token != "new_token_for_alphrad"), token => Assert.False(token.Active));
    }
    
    
}