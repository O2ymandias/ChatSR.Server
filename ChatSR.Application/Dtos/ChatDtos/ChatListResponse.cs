namespace ChatSR.Application.Dtos.ChatDtos;

public record ChatListResponse(
	Guid ChatId,
	string Name,
	bool IsGroup,
	DateTimeOffset CreatedAt,
	int MemberCount,
	LastMessageOverview? LastMessageOverview,
	string? DisplayPictureUrl,
	int UnreadCount
);

public record LastMessageOverview(
	Guid MessageId,
	string SenderId,
	string Content,
	string SenderDisplayName,
	DateTimeOffset SentAt,
	bool IsRead
);