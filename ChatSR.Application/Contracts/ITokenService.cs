using ChatSR.Infrastructure.Entities;

namespace ChatSR.Application.Contracts;

public interface ITokenService
{
	Task<string> GenerateTokenAsync(User user);
}
