using ChatSR.Application.Dtos.AuthDtos;

namespace ChatSR.Application.Contracts;

public interface IAuthService
{
	Task<AuthResponse> RegisterUserAsync(RegisterUserRequest request);
	Task<AuthResponse> LoginUserAsync(LoginUserRequest request);
}
