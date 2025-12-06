using ChatSR.Api.Extensions;
using ChatSR.Application.Contracts;
using ChatSR.Application.Dtos.AuthDtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatSR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<IActionResult> Register(RegisterUserRequest request)
	{
		var result = await authService.RegisterUserAsync(request);
		return result.ToActionResult();
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginUserRequest request)
	{
		var result = await authService.LoginUserAsync(request);
		return result.ToActionResult();
	}
}
