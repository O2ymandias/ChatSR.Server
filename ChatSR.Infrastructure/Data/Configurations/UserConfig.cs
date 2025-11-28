using ChatSR.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatSR.Infrastructure.Data.Configurations;

internal class UserConfig : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder
			.Property(u => u.DisplayName)
			.IsRequired()
			.HasMaxLength(100);

		builder
			.Property(u => u.PictureUrl)
			.IsRequired(false);

		builder
			.Property(u => u.CreatedAt)
			.IsRequired();

		builder
			.Property(u => u.LastActive)
			.IsRequired(false);

		// Global query filter to exclude soft-deleted users
		builder.HasQueryFilter(u => !u.IsDeleted);

		builder
			.HasMany(u => u.ChatMembers)
			.WithOne(cm => cm.User)
			.HasForeignKey(cm => cm.UserId)
			.OnDelete(DeleteBehavior.Restrict);

		builder
			.HasMany(u => u.Messages)
			.WithOne(m => m.User)
			.HasForeignKey(m => m.UserId)
			.OnDelete(DeleteBehavior.Restrict);

	}
}
