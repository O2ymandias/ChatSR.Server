namespace ChatSR.Application.Dtos.MessageDtos;

public record MessageResponse(
	Guid MessageId,
	Guid chatId,
	string Content,
	DateTimeOffset SentAt,
	bool IsEdited,
	DateTimeOffset? EditedAt,
	string SenderId,
	string SenderDisplayName,
	string? SenderPictureUrl
);
