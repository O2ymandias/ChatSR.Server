using ChatSR.Infrastructure.Shared.Enums;

namespace ChatSR.Infrastructure.Entities;

public class ChatMember
{
	public ChatMemberRole Role { get; set; }
	public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? LastReadAt { get; set; }

	// One-to-Many: Chat - ChatMembers
	public Chat Chat { get; set; }
	public Guid ChatId { get; set; }

	// One-to-Many: AppUser - ChatMembers
	public User User { get; set; }
	public string UserId { get; set; }
}
