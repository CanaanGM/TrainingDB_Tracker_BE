namespace API.Security;

public class JwtSettings
{
	public const string JwtSettingsSection = "JwtSettings";
	public string Issuer { get; init; } = null!;
	public int ExpiryMinutes { get; init; }
	public string Audience { get; init; } = null!;
	public string Secret { get; init; } = null!;
}