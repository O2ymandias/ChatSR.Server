using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos.ChatMemberDtos;

public record CreateChatRequest
{
	[Required]
	public bool IsGroup { get; init; }

	[Required]
	[MaxLength(5, ErrorMessage = "You can add at most 5 members.")]
	[MinLength(1, ErrorMessage = "You must add at least 1 member.")]
	public List<string> MemberIds { get; init; }

	public string? Name { get; init; }
	public IFormFile? Picture { get; init; }
}
