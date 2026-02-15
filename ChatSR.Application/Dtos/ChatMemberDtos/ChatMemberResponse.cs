namespace ChatSR.Application.Dtos.ChatMemberDtos;

public record ChatMemberResponse(
	Guid ChatId,
	string UserId,
	string DisplayName,
	string? PictureUrl,
	string Role,
	DateTimeOffset JoinedAt
);
