namespace ChatSR.Application.Dtos.ChatMemberDtos;

public record ChatResponse(
	Guid ChatId,
	string? Name,
	bool IsGroup,
	DateTime CreatedAt
);