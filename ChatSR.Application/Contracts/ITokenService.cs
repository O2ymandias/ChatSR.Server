using ChatSR.Infrastructure.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace ChatSR.Application.Contracts;

public interface ITokenService
{
	Task<JwtSecurityToken> GenerateTokenAsync(User user);
}
