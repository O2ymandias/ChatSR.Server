namespace ChatSR.Application.Dtos.MessageDtos;

public record MessageResponse(
	Guid MessageId,
	string Content,
	DateTime SentAt,
	bool IsEdited,
	DateTime? EditedAt,
	string SenderId,
	string SenderDisplayName,
	string? SenderPictureUrl
);
