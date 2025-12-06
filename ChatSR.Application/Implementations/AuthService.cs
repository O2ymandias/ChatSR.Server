using ChatSR.Application.Contracts;
using ChatSR.Application.Dtos.AuthDtos;
using ChatSR.Application.Shared.Constants;
using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;
using ChatSR.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace ChatSR.Application.Implementations;

public class AuthService(UserManager<User> userManager, ITokenService tokenService) : IAuthService
{
	public async Task<Result<AuthResponse>> RegisterUserAsync(RegisterUserRequest request)
	{
		var emailAlreadyExists = await userManager.FindByEmailAsync(request.Email);
		if (emailAlreadyExists is not null)
			return Result<AuthResponse>.Failure(Error.Conflict("Email already exists."));

		var userNameAlreadyExists = await userManager.FindByNameAsync(request.UserName);
		if (userNameAlreadyExists is not null)
			return Result<AuthResponse>.Failure(Error.Conflict("UserName already exists."));

		var user = new User()
		{
			UserName = request.UserName,
			Email = request.Email,
			DisplayName = request.UserName,
			PhoneNumber = request.Phone,
		};

		var creationResult = await userManager.CreateAsync(user, request.Password);

		if (!creationResult.Succeeded)
		{
			var errors = string.Join(", ", creationResult.Errors.Select(e => e.Description));
			return Result<AuthResponse>.Failure(Error.Validation(errors));
		}

		var assignToRoleResult = await userManager.AddToRoleAsync(user, RoleConstants.User);
		if (!assignToRoleResult.Succeeded)
		{
			var errors = string.Join(", ", assignToRoleResult.Errors.Select(e => e.Description));
			return Result<AuthResponse>.Failure(Error.Validation(errors));
		}

		var jwtSecurityToken = await tokenService.GenerateTokenAsync(user);
		var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

		var basicUserInfo = new BasicUserInfo(
			user.UserName,
			user.DisplayName,
			user.Email,
			[RoleConstants.User]
		);

		return Result<AuthResponse>.Success(new AuthResponse(basicUserInfo, token, jwtSecurityToken.ValidTo));
	}

	public async Task<Result<AuthResponse>> LoginUserAsync(LoginUserRequest request)
	{
		var user = request.UserNameOrEmail.Contains('@')
			? await userManager.FindByEmailAsync(request.UserNameOrEmail)
			: await userManager.FindByNameAsync(request.UserNameOrEmail);

		if (user is null)
			return Result<AuthResponse>.Failure(Error.Validation("Incorrect email/userName or password"));

		if (!await userManager.CheckPasswordAsync(user, request.Password))
			return Result<AuthResponse>.Failure(Error.Validation("Incorrect email/userName or password"));

		var jwtSecurityToken = await tokenService.GenerateTokenAsync(user);
		var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

		var userRoles = await userManager.GetRolesAsync(user);

		var userInfo = new BasicUserInfo(
			user.UserName ?? string.Empty,
			user.DisplayName,
			user.Email ?? string.Empty,
			[.. userRoles]
		);

		return Result<AuthResponse>.Success(new AuthResponse(userInfo, token, jwtSecurityToken.ValidTo));
	}
}
