namespace ChatSR.Application.Dtos.ChatMemberDtos;

public record MessageResponse(
	Guid Id,
	string Content,
	DateTime SentAt,
	string UserId,
	string UserDisplayName
);
