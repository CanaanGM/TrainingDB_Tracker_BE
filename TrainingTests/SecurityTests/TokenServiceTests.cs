using API.Common.Providers;
using API.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace TrainingTests.SecurityTests;

public class TokenServiceTests
{
	private readonly TokenService _tokenService;

	public TokenServiceTests()
	{
		var jwtSettings = new JwtSettings
		{
			Secret = "what is a man? a miserable little pile of secrets, but enough talk, HAVE AT YOU!",
			Issuer = "http://localhost",
			Audience = "http://localhost",
			ExpiryMinutes = 1350
		};
		var mockOptions = new Mock<IOptions<JwtSettings>>();
		mockOptions.Setup(m => m.Value).Returns(jwtSettings);
		
		var iDateTimeProviderMock = new  Mock<IDateTimeProvider>();
		iDateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
		
		_tokenService = new TokenService(mockOptions.Object, iDateTimeProviderMock.Object);
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
		string padded = input.Length % 4 == 0 ? input : input + "====".Substring(input.Length % 4);
		string base64 = padded.Replace("_", "/")
			.Replace("-", "+");
		return Convert.FromBase64String(base64);
	}
}