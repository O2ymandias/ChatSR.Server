namespace ChatSR.Application.Dtos.ChatDtos;

public record ChatResponse(
	Guid ChatId,
	bool IsGroup,
	DateTime CreatedAt,
	string? Name,
	string? DisplayPictureUrl
);