using ChatSR.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatSR.Infrastructure.Data.Configurations;

internal class ChatConfig : IEntityTypeConfiguration<Chat>
{
	public void Configure(EntityTypeBuilder<Chat> builder)
	{
		builder.ToTable("Chats");

		builder.HasKey(c => c.Id);

		builder
			.Property(c => c.Name)
			.IsRequired(false)
			.HasMaxLength(100);

		builder
			.Property(c => c.IsGroup)
			.IsRequired();

		builder
			.Property(c => c.CreatedAt)
			.IsRequired();

		builder
			.HasMany(c => c.ChatMembers)
			.WithOne(cm => cm.Chat)
			.HasForeignKey(cm => cm.ChatId)
			.OnDelete(DeleteBehavior.Cascade);

		builder
			.HasMany(c => c.Messages)
			.WithOne(m => m.Chat)
			.HasForeignKey(m => m.ChatId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
