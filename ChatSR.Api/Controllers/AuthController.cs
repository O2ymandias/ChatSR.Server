using ChatSR.Application.Contracts;
using ChatSR.Application.Dtos.AuthDtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatSR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult<AuthResponse>> Register(RegisterUserRequest request)
	{
		var result = await authService.RegisterUserAsync(request);
		return Ok(result);
	}

	[HttpPost("login")]
	public async Task<ActionResult<AuthResponse>> Login(LoginUserRequest request)
	{
		var result = await authService.LoginUserAsync(request);
		return Ok(result);
	}

}
