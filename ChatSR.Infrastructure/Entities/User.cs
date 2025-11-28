using Microsoft.AspNetCore.Identity;

namespace ChatSR.Infrastructure.Entities;

public class User : IdentityUser
{
	public string DisplayName { get; set; }
	public string? PictureUrl { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? LastActive { get; set; }
	public bool IsDeleted { get; set; } = false;

	// One-to-Many: Chat - ChatMembers
	public ICollection<ChatMember> ChatMembers { get; set; }

	// One-to-Many: AppUser - Messages
	public ICollection<Message> Messages { get; set; }

}
