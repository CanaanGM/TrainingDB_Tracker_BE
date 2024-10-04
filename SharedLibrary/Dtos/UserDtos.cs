using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Dtos;

public class UserWriteDto
{
    public required string Name { get; set; }
    [EmailAddress] public required string Email { get; set; }
    [MinLength(8)][MaxLength(512)] public required string Password { get; set; }
    public double? Height { get; set; }
    public string? Gender { get; set; }
    public string? Image { get; set; }
}

public class UserAuthDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; }
    public string Token { get; set; }

}
public class InternalUserAuthDto : UserAuthDto
{
    public int Id { get; set; }
    public string LatestPasswordHash { get; set; }
    public List<RefreshTokenReadDto> RefreshTokens { get; set; }
}



public class RefreshTokenReadDto
{
    public string Token { get; set; } = null!;
    public DateTime? Expires { get; set; }

    public DateTime? Revoked { get; set; }

    public bool? Active { get; set; }
}

public class UserLogInDto
{
    [EmailAddress] public required string Email { get; set; }
    [MinLength(8)][MaxLength(512)] public required string Password { get; set; }
}