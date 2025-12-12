namespace ChatSR.Application.Dtos.ChatMemberDtos;

public record ChatMemberResponse(
	string UserId,
	string DisplayName,
	string? PictureUrl,
	string Role,
	DateTime JoinedAt
);
