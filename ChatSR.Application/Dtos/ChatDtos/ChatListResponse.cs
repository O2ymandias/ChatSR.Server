using ChatSR.Application.Dtos.MessageDtos;

namespace ChatSR.Application.Dtos.ChatDtos;

public record ChatListResponse(
	Guid ChatId,
	string Name,
	bool IsGroup,
	DateTime CreatedAt,
	int MemberCount,
	MessageResponse? LastMessage
);
