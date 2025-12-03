using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos.AuthDtos;

public record LoginUserRequest
{
	[Required] public string UserNameOrEmail { get; init; }
	[Required] public string Password { get; init; }
}
