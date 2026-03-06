using ChatSR.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatSR.Infrastructure.Data.Configurations;

internal class MessageConfig : IEntityTypeConfiguration<Message>
{
	public void Configure(EntityTypeBuilder<Message> builder)
	{
		builder.ToTable("Messages");

		builder.HasKey(m => m.Id);

		builder
			.Property(m => m.Content)
			.IsRequired()
			.HasMaxLength(2000);

		builder
			.Property(m => m.SentAt)
			.IsRequired();

		builder
			.Property(m => m.IsEdited)
			.IsRequired();

		builder
			.Property(m => m.EditedAt)
			.IsRequired(false);

		// Configuring the child. why?
		// Because the physical table in database has a FK which means it's the child table 
		builder.
			HasOne(c => c.ReplyToMessage) // 1 c -> 1 p
			.WithMany(p => p.Replies) // 1 p -> M c
			.HasForeignKey(c => c.ReplyToMessageId);

		/*
			Ideally we would use DeleteBehavior.SetNull so that when the parent
			message is deleted, ReplyToMessageId in the replies becomes NULL.

			However, SQL Server doesn't support SET NULL (or CASCADE) on
			self-referencing foreign keys as it can introduce cycles or multiple
			cascade paths.

			Because of this, EF Core defaults to Restrict for self-referencing
			relationships on SQL Server, meaning deleting a parent message will
			be BLOCKED if any replies still reference it.

			To handle this properly, we manually null out ReplyToMessageId
			on all replies before deleting the parent message.
		*/
	}
}
