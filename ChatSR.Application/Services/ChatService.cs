using ChatSR.Application.Dtos.ChatDtos;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Dtos.MessageDtos;
using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;
using ChatSR.Infrastructure.Data;
using ChatSR.Infrastructure.Entities;
using ChatSR.Infrastructure.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatSR.Application.Services;

public class ChatService(
	AppDbContext dbContext,
	UserManager<User> userManager,
	IImageUploader imageUploader
) : IChatService
{
	public async Task<Result<ChatResponse>> CreateChatAsync(string currentUserId, CreateChatRequest request)
	{
		if (request.IsGroup && string.IsNullOrWhiteSpace(request.Name))
		{
			return Result<ChatResponse>.Failure(
				Error.Validation("Group chats must have a name.")
			);
		}

		var memberIds = request.MemberIds.ToList();

		if (!memberIds.Contains(currentUserId))
			memberIds.Add(currentUserId);

		if (!request.IsGroup && memberIds.Count != 2)
		{
			return Result<ChatResponse>.Failure(
				Error.Validation("Direct chat must have exactly 2 members.")
			);
		}

		var validUsersIds = await userManager.Users
			.Where(u => memberIds.Contains(u.Id))
			.Select(u => u.Id)
			.ToListAsync();

		if (validUsersIds.Count != memberIds.Count)
		{
			var invalidIds = memberIds.Except(validUsersIds);
			return Result<ChatResponse>.Failure(
				Error.Validation($"Invalid memberIds: {string.Join(", ", invalidIds)}")
			);
		}

		// 1:1 Chat
		if (!request.IsGroup)
		{
			var otherUserId = memberIds.First(id => id != currentUserId);

			var existingChat = await dbContext.Chats
				.Where(c =>
					!c.IsGroup &&
					c.ChatMembers.Any(cm => cm.UserId == currentUserId) &&
					c.ChatMembers.Any(cm => cm.UserId == otherUserId)
				)
				.FirstOrDefaultAsync();

			if (existingChat is not null)
			{
				return await GetChatByIdAsync(currentUserId, existingChat.Id);
			}
		}

		var newChat = new Chat()
		{
			Id = Guid.NewGuid(),
			IsGroup = request.IsGroup,
			ChatMembers = [],
			Name = request.Name,
			DisplayPictureUrl = await imageUploader.UploadImageAsync(request.Picture, "images/chats")
		};

		foreach (var memberId in memberIds)
		{
			var chatMember = new ChatMember()
			{
				UserId = memberId,
				Role = request.IsGroup
					? memberId == currentUserId ? ChatMemberRole.Admin : ChatMemberRole.Member
					: ChatMemberRole.Member,
			};
			newChat.ChatMembers.Add(chatMember);
		}

		dbContext.Chats.Add(newChat);

		await dbContext.SaveChangesAsync();

		return await GetChatByIdAsync(currentUserId, newChat.Id);
	}

	public async Task<Result<ChatResponse>> GetChatByIdAsync(string currentUserId, Guid chatId)
	{
		var chat = await dbContext.Chats
			.Where(c =>
				c.Id == chatId &&
				c.ChatMembers.Any(cm => cm.UserId == currentUserId)
			)
			.Select(c => new ChatResponse(
				c.Id,
				c.IsGroup,
				c.CreatedAt,
					c.IsGroup
					? c.Name ?? string.Empty
					: (c.ChatMembers
						.Where(cm => cm.UserId != currentUserId)
						.Select(cm => cm.User.DisplayName)
						.FirstOrDefault()) ?? string.Empty,
				c.DisplayPictureUrl
			)
			)
			.FirstOrDefaultAsync();

		if (chat is null)
		{
			return Result<ChatResponse>.Failure(
				Error.NotFound($"Unable to access this chat.")
			);
		}

		return Result<ChatResponse>.Success(chat);
	}

	public async Task<Result<List<ChatListResponse>>> GetUserChatsAsync(string currentUserId)
	{
		var chats = await dbContext.Chats
			// Get only chats where the current user is a member
			.Where(c => c.ChatMembers.Any(cm => cm.UserId == currentUserId))
			.Select(c => new ChatListResponse(
				c.Id,

				// For group chats use the chat name,
				// For 1:1 chats use the OTHER member's display name
				c.IsGroup
					? c.Name ?? string.Empty
					: (c.ChatMembers
						.Where(cm => cm.UserId != currentUserId)
						.Select(cm => cm.User.DisplayName)
						.FirstOrDefault()) ?? string.Empty,

				c.IsGroup,
				c.CreatedAt,

				// Total number of members in this chat
				c.ChatMembers.Count,

				// Last message preview (take the most recent message only)
				c.Messages
					.OrderByDescending(m => m.SentAt)
					.Select(m => new MessageResponse(
						m.Id,
						m.ChatId,
						m.Content,
						m.SentAt,
						m.IsEdited,
						m.EditedAt,
						m.UserId,
						m.User.DisplayName,
						m.User.PictureUrl,
						// True: at least one member other than the sender has read this message
						c.ChatMembers.Any(cm =>
							cm.UserId != m.UserId &&    // Exclude the message sender
							cm.LastReadAt >= m.SentAt
						)
					))
					.FirstOrDefault(),

				c.DisplayPictureUrl,

				// Count messages sent AFTER the current user's LastReadAt
				c.Messages.Count(m =>
					m.UserId != currentUserId &&   // Exclude messages sent by the current user.

					// Message's sentAt > ChatMember's (Current user in this chat) lastReadAt
					m.SentAt > (
							c.ChatMembers
							.Where(cm => cm.UserId == currentUserId)    // Find the current user's ChatMember row (always one row)
							.Select(cm => cm.LastReadAt)
							.FirstOrDefault() ?? DateTimeOffset.MinValue
						)
				)
			))
			.ToListAsync();

		return Result<List<ChatListResponse>>.Success(chats);
	}

	public async Task<DateTimeOffset?> MarkChatAsReadAsync(string currentUserId, Guid chatId)
	{
		var chatMember = await dbContext.ChatMembers
			.FirstOrDefaultAsync(cm =>
				cm.UserId == currentUserId &&
				cm.ChatId == chatId
			);

		if (chatMember is null) return null;

		chatMember.LastReadAt = DateTimeOffset.UtcNow;
		await dbContext.SaveChangesAsync();

		return chatMember.LastReadAt;
	}

	public async Task<Result> AddMembersAsync(string currentUserId, Guid chatId, AddMembersRequest request)
	{
		var chat = await dbContext.Chats
			.Include(c => c.ChatMembers)
			.FirstOrDefaultAsync(c =>
				c.Id == chatId &&
				c.ChatMembers.Any(cm => cm.UserId == currentUserId)
			);

		if (chat is null)
		{
			return Result.Failure(
				Error.NotFound("Unable to access this chat.")
			);
		}

		if (!chat.IsGroup)
		{
			return Result.Failure(
				Error.Validation("You can't add members to a private chat.")
			);
		}

		var validUsersIds = await userManager.Users
			.Where(u => request.MemberIds.Contains(u.Id))
			.Select(u => u.Id)
			.ToListAsync();

		if (validUsersIds.Count != request.MemberIds.Count)
		{
			var invalidIds = request.MemberIds.Except(validUsersIds);
			return Result.Failure(
				Error.Validation($"Invalid memberIds: {string.Join(", ", invalidIds)}")
			);
		}

		foreach (var memberId in validUsersIds)
		{
			if (chat.ChatMembers.Any(cm => cm.UserId == memberId))
				continue;

			chat.ChatMembers.Add(new ChatMember()
			{
				UserId = memberId,
				Role = ChatMemberRole.Member
			});
		}

		await dbContext.SaveChangesAsync();

		return Result.Success();
	}

	public async Task<Result> RemoveMemberAsync(string currentUserId, Guid chatId, string memberIdToRemove)
	{
		if (currentUserId == memberIdToRemove)
		{
			return Result.Failure(
				Error.Validation("You can't remove yourself. call 'GET: /api/chats/leave-chat' to leave chat")
			);
		}

		var userToRemove = await userManager.FindByIdAsync(memberIdToRemove);
		if (userToRemove is null)
		{
			return Result.Failure(
				Error.NotFound($"The user '{memberIdToRemove}' does not exist.")
			);
		}

		var chat = await dbContext.Chats
			.Include(c => c.ChatMembers)
			.FirstOrDefaultAsync(c =>
				c.Id == chatId &&
				c.ChatMembers.Any(cm => cm.UserId == currentUserId)
			);

		if (chat is null)
		{
			return Result.Failure(
				Error.NotFound("Unable to access this chat.")
			);
		}

		if (!chat.IsGroup)
		{
			return Result.Failure(
				Error.Validation("You can't remove members from a private chat.")
			);
		}

		var isAdmin = chat.ChatMembers.Any(cm =>
			cm.UserId == currentUserId &&
			cm.Role == ChatMemberRole.Admin
		);

		if (!isAdmin)
		{
			return Result.Failure(
				Error.Validation("You do not have permission to remove members from this chat.")
			);
		}

		var memberToRemove = chat.ChatMembers.FirstOrDefault(cm => cm.UserId == memberIdToRemove);
		if (memberToRemove is null)
		{
			return Result.Failure(
				Error.NotFound($"The user '{memberIdToRemove}' is not a member of this chat.")
			);
		}

		chat.ChatMembers.Remove(memberToRemove);

		await dbContext.SaveChangesAsync();

		return Result.Success();
	}

	public async Task<Result> LeaveChatAsync(string currentUserId, Guid chatId)
	{
		var chat = await dbContext.Chats
			.Include(cm => cm.ChatMembers)
			.FirstOrDefaultAsync(c => c.Id == chatId);

		if (chat is null)
		{
			return Result.Failure(
				Error.NotFound($"No chat was found with the provided ID: {chatId}")
			);
		}

		var member = chat.ChatMembers.FirstOrDefault(cm => cm.UserId == currentUserId);
		if (member is null)
		{
			return Result.Failure(
				Error.NotFound($"The user '{currentUserId}' is not a member of this chat.")
			);
		}

		if (member.Role == ChatMemberRole.Admin)
		{
			var newAdmin = chat.ChatMembers.FirstOrDefault(cm => cm.UserId != member.UserId);

			if (newAdmin is not null)
				newAdmin.Role = ChatMemberRole.Admin;
			else
				dbContext.Chats.Remove(chat);
		}

		dbContext.ChatMembers.Remove(member);

		await dbContext.SaveChangesAsync();

		return Result.Success();
	}

	public async Task<bool> IsChatMemberAsync(string currentUserId, Guid chatId)
	{
		return await dbContext.ChatMembers.
			AnyAsync(cm =>
				cm.ChatId == chatId &&
				cm.UserId == currentUserId
			);
	}

	public Task<List<string>> GetChatMemberIdsAsync(Guid chatId)
	{
		var memberIds = dbContext.ChatMembers
			.Where(cm => cm.ChatId == chatId)
			.Select(cm => cm.UserId)
			.ToListAsync();

		return memberIds;
	}

	public async Task<Result<List<ChatMemberResponse>>> GetChatMembers(Guid chatId)
	{
		var chatMembers = await dbContext.ChatMembers
			.Where(cm => cm.ChatId == chatId)
			.Select(cm => new ChatMemberResponse(
				cm.ChatId,
				cm.UserId,
				cm.User.DisplayName,
				cm.User.PictureUrl,
				cm.Role.ToString(),
				cm.JoinedAt
			))
			.ToListAsync();

		return Result<List<ChatMemberResponse>>.Success(chatMembers);
	}

	public async Task<List<string>> GetSharedMemberIdsAsync(string userId)
	{
		return await dbContext.ChatMembers
			.Where(cm => cm.UserId == userId)
			.Select(cm => cm.Chat)
			.SelectMany(c => c.ChatMembers)
			.Where(cm => cm.UserId != userId)
			.Select(cm => cm.UserId)
			.Distinct()
			.ToListAsync();
	}
}
