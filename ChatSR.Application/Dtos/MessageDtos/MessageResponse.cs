namespace ChatSR.Application.Dtos.MessageDtos;

public record MessageResponse(
	Guid MessageId,
	Guid ChatId,
	string Content,
	DateTimeOffset SentAt,
	bool IsEdited,
	DateTimeOffset? EditedAt,
	string SenderId,
	string SenderDisplayName,
	string? SenderPictureUrl,
	bool IsRead,
	ReplyToOverview? ReplyTo
);

public record ReplyToOverview(Guid MessageId, string Content, string SenderDisplayName);
