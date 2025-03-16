using DataLibrary.Models;

using Microsoft.IdentityModel.Tokens;

using SharedLibrary.Dtos;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Common.Providers;
using Microsoft.Extensions.Options;

namespace API.Security;

public interface ITokenService
{
    string CreateToken(InternalUserAuthDto internalUser);
    RefreshToken GenerateRefreshToken();
}

public class TokenService : ITokenService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private const int RefreshTokenSize = 64;
    private readonly JwtSettings _jwtSettings;
    public TokenService(IOptions<JwtSettings> jwtOptions, IDateTimeProvider dateTimeProvider)
    {
	    _jwtSettings = jwtOptions.Value;	
	    _dateTimeProvider = dateTimeProvider;
    }

    public string CreateToken(InternalUserAuthDto internalUser)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, internalUser.Username),
            new Claim(ClaimTypes.NameIdentifier, internalUser.Id.ToString()),
            new Claim(ClaimTypes.Email, internalUser.Email!),
        };

        claims.AddRange(internalUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _jwtSettings.Audience,
			Issuer = _jwtSettings.Issuer,
            SigningCredentials = creds,
            NotBefore = _dateTimeProvider.UtcNow,
            Expires = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            IssuedAt = _dateTimeProvider.UtcNow
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken()
    {
        var randNumber = new byte[RefreshTokenSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randNumber);

        return new RefreshToken
        {
            Token = Base64UrlEncode(randNumber),
            Expires = DateTime.UtcNow.AddDays(7), // Token expiration, typically 7-30 days
            Revoked = null,
            Active = true
        };
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}