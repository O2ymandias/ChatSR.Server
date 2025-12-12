using ChatSR.Infrastructure.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace ChatSR.Application.Interfaces;

public interface ITokenService
{
	Task<JwtSecurityToken> GenerateTokenAsync(User user);
}
