using ChatSR.Application.Dtos.MessageDtos;

namespace ChatSR.Application.Dtos.ChatDtos;

public record ChatListResponse(
	Guid ChatId,
	string Name,
	bool IsGroup,
	DateTimeOffset CreatedAt,
	int MemberCount,
	MessageResponse? LastMessage,
	string? DisplayPictureUrl
);
