using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos.AuthDtos;

public record RegisterUserRequest
{
	[Required]
	[MinLength(3)]
	[MaxLength(100)]
	public string UserName { get; init; }

	[Required]
	[EmailAddress]
	public string Email { get; init; }

	[Required]
	[RegularExpression(
	@"^01[0125][0-9]{8}$",
	ErrorMessage = "Invalid Egyptian mobile number. It must start with 010, 011, 012, or 015 and contain 11 digits."
	)]
	public string Phone { get; init; }

	[Required]
	[RegularExpression(
	@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
	ErrorMessage = "Password must be at least 6 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character (@, $, !, %, *, ?, &)."
	)]
	public string Password { get; init; }
}
