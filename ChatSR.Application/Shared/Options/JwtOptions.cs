namespace ChatSR.Application.Shared.Options;

public class JwtOptions
{
	public string Issuer { get; set; }
	public string Audience { get; set; }
	public int ExpiryInMinutes { get; set; }
	public string SecretKey { get; set; }
}
