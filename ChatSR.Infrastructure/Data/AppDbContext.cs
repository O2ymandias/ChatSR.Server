using ChatSR.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatSR.Infrastructure.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
	public DbSet<Chat> Chats { get; set; }
	public DbSet<ChatMember> ChatMembers { get; set; }
	public DbSet<Message> Messages { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
