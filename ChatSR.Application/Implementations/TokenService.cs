using ChatSR.Application.Contracts;
using ChatSR.Application.Shared.Options;
using ChatSR.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatSR.Application.Implementations;

public class TokenService(IOptions<JwtOptions> jwtOptions, UserManager<User> userManager) : ITokenService
{
	private readonly JwtOptions _jwtOptions = jwtOptions.Value;

	public async Task<string> GenerateTokenAsync(User user)
	{
		List<Claim> claims = [
			// [1] Registered JWT Claims
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),

			// [2] Public Claims (Standard .NET ClaimTypes)
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
			new Claim(ClaimTypes.Email, user.Email ?? string.Empty),

			// [3] Private Claims (Application-specific)
			new Claim("displayName", user.DisplayName ?? string.Empty),
		];

		// Add user-level claims from ASP.NET Identity
		var userClaims = await userManager.GetClaimsAsync(user);
		claims.AddRange(userClaims);

		// Add roles as claims
		var userRoles = await userManager.GetRolesAsync(user);
		claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

		// Create signing credentials
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
		var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		// Build the JWT
		var token = new JwtSecurityToken(
			issuer: _jwtOptions.Issuer,
			audience: _jwtOptions.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryInMinutes),
			signingCredentials: signingCredentials
		);
		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
