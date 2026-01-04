namespace ChatSR.Application.Dtos.ChatDtos;

public record ChatResponse(
	Guid ChatId,
	bool IsGroup,
	DateTimeOffset CreatedAt,
	string? Name,
	string? DisplayPictureUrl
);