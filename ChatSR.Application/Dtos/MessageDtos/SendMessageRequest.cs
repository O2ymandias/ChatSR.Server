using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos.MessageDtos;

public record SendMessageRequest
{
	[Required]
	[MinLength(1)]
	[MaxLength(2000)]
	public string Content { get; init; }
}
