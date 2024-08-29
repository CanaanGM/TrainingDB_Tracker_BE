using API.Security;
using Microsoft.Extensions.Configuration;

namespace TrainingTests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { { "TokenKey", "YourSuperSecretKey123!" } })
            .Build();

        _tokenService = new TokenService(config);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateValidToken()
    {
        var refreshToken = _tokenService.GenerateRefreshToken();

        Assert.NotNull(refreshToken);
        Assert.False(string.IsNullOrEmpty(refreshToken.Token));
        Assert.NotNull((object)refreshToken.Expires);
        Assert.True(refreshToken.Active.Value);
        Assert.Null((object)refreshToken.Revoked);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        var refreshToken1 = _tokenService.GenerateRefreshToken();
        var refreshToken2 = _tokenService.GenerateRefreshToken();

        Assert.NotEqual(refreshToken1.Token, refreshToken2.Token);
    }

    private byte[] Base64UrlDecode(string input)
    {
        string padded = input.Length % 4 == 0 ? input :
            input + "====".Substring(input.Length % 4);
        string base64 = padded.Replace("_", "/")
            .Replace("-", "+");
        return Convert.FromBase64String(base64);
    }
}