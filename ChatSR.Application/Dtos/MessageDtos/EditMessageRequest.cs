using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos.MessageDtos;

public record EditMessageRequest
{
	[Required]
	[MinLength(1)]
	[MaxLength(2000)]
	public string NewContent { get; init; }
}
