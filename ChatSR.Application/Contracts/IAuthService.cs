using ChatSR.Application.Dtos.AuthDtos;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Application.Contracts;

public interface IAuthService
{
	Task<Result<AuthResponse>> RegisterUserAsync(RegisterUserRequest request);
	Task<Result<AuthResponse>> LoginUserAsync(LoginUserRequest request);
}
