namespace ChatSR.Infrastructure.Entities;

public class Chat
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public bool IsGroup { get; set; }
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	public string? DisplayPictureUrl { get; set; }


	// One-to-Many: Chat - ChatMembers
	public ICollection<ChatMember> ChatMembers { get; set; }

	// One-to-Many: Chat - Messages
	public ICollection<Message> Messages { get; set; }
}
