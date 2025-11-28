using ChatSR.Infrastructure.Entities;
using ChatSR.Infrastructure.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatSR.Infrastructure.Data.Configurations;

internal class ChatMemberConfig : IEntityTypeConfiguration<ChatMember>
{
	public void Configure(EntityTypeBuilder<ChatMember> builder)
	{
		builder.ToTable("ChatMembers");

		builder.HasKey(cm => new { cm.ChatId, cm.UserId });

		builder.Property(cm => cm.JoinedAt)
			.IsRequired();

		builder
			.Property(cm => cm.Role)
			.IsRequired()
			.HasConversion(
				to => to.ToString(),
				from => Enum.Parse<ChatMemberRole>(from)
			);
	}
}
