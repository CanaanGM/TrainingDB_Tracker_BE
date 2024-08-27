using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataLibrary.Models;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Dtos;

namespace API.Security;
public class TokenService
{
    private readonly IConfiguration _config;
    private const int RefreshTokenSize = 64;
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(UserAuthDto user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            // new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            //Expires = DateTime.UtcNow.AddMinutes(15),
            Expires = DateTime.UtcNow.AddDays(1), // dev only
            SigningCredentials = creds
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