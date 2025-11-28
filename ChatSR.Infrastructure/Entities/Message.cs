namespace ChatSR.Infrastructure.Entities;

public class Message
{
	public Guid Id { get; set; }
	public string Content { get; set; }
	public DateTime SentAt { get; set; } = DateTime.UtcNow;

	// One-to-Many: Chat - Messages
	public Chat Chat { get; set; }
	public Guid ChatId { get; set; }

	// One-to-Many: AppUser - Messages
	public User User { get; set; }
	public string UserId { get; set; }
}
