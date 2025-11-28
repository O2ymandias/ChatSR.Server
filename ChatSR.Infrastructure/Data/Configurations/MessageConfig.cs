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
	}
}
