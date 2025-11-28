namespace ChatSR.Infrastructure.Entities;

public class Chat
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public bool IsGroup { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


	// One-to-Many: Chat - ChatMembers
	public ICollection<ChatMember> ChatMembers { get; set; }

	// One-to-Many: Chat - Messages
	public ICollection<Message> Messages { get; set; }
}
