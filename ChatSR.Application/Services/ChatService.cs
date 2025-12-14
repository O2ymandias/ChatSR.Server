using ChatSR.Application.Dtos.ChatDtos;
using ChatSR.Application.Dtos.ChatMemberDtos;
using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;
using ChatSR.Infrastructure.Data;
using ChatSR.Infrastructure.Entities;
using ChatSR.Infrastructure.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatSR.Application.Services;

public class ChatService(AppDbContext dbContext, UserManager<User> userManager) : IChatService
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
				return Result<ChatResponse>.Success(
					new ChatResponse(existingChat.Id, existingChat.Name, existingChat.IsGroup, existingChat.CreatedAt)
				);
			}
		}

		var newChat = new Chat()
		{
			Id = Guid.NewGuid(),
			Name = request.Name,
			IsGroup = request.IsGroup,
			ChatMembers = []
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

		return Result<ChatResponse>.Success(
			new ChatResponse(newChat.Id, newChat.Name, newChat.IsGroup, newChat.CreatedAt)
		);

	}

	public async Task<Result<ChatResponse>> GetChatByIdAsync(string currentUserId, Guid chatId)
	{
		var chat = await dbContext.Chats
			.Where(c =>
				c.Id == chatId &&
				c.ChatMembers.Any(cm => cm.UserId == currentUserId)
			)
			.Select(c => new ChatResponse(c.Id, c.Name, c.IsGroup, c.CreatedAt))
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
			.Where(c => c.ChatMembers.Any(cm => cm.UserId == currentUserId))
			.Select(c => new ChatListResponse(
				c.Id,
				c.IsGroup
					? c.Name
					: c.ChatMembers
						.Where(cm => cm.UserId != currentUserId)
						.Select(cm => cm.User.DisplayName)
						.FirstOrDefault(),
				c.IsGroup,
				c.CreatedAt,
				c.ChatMembers.Count
			))
			.ToListAsync();

		return Result<List<ChatListResponse>>.Success(chats);
	}

	public async Task<Result<ChatResponse>> AddMembersAsync(string currentUserId, Guid chatId, AddMembersRequest request)
	{
		var chat = await dbContext.Chats
			.Include(c => c.ChatMembers)
			.FirstOrDefaultAsync(c =>
				c.Id == chatId &&
				c.ChatMembers.Any(cm => cm.UserId == currentUserId)
			);

		if (chat is null)
		{
			return Result<ChatResponse>.Failure(
				Error.NotFound("Unable to access this chat.")
			);
		}

		if (!chat.IsGroup)
		{
			return Result<ChatResponse>.Failure(
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
			return Result<ChatResponse>.Failure(
				Error.Validation($"Invalid memberIds: {string.Join(", ", invalidIds)}")
			);
		}

		bool added = false;
		foreach (var memberId in validUsersIds)
		{
			if (chat.ChatMembers.Any(cm => cm.UserId == memberId))
				continue;

			chat.ChatMembers.Add(new ChatMember()
			{
				UserId = memberId,
				Role = ChatMemberRole.Member
			});

			added = true;
		}

		if (!added)
		{
			return Result<ChatResponse>.Success(
				new ChatResponse(chat.Id, chat.Name, chat.IsGroup, chat.CreatedAt)
			);
		}

		await dbContext.SaveChangesAsync();

		return Result<ChatResponse>.Success(
			new ChatResponse(chat.Id, chat.Name, chat.IsGroup, chat.CreatedAt)
		);
	}

	public async Task<Result<ChatResponse>> RemoveMemberAsync(string currentUserId, Guid chatId, string memberIdToRemove)
	{
		if (currentUserId == memberIdToRemove)
		{
			return Result<ChatResponse>.Failure(
				Error.Validation("You can't remove yourself. call 'GET: /api/chats/leave-chat' to leave chat")
			);
		}

		var userToRemove = await userManager.FindByIdAsync(memberIdToRemove);
		if (userToRemove is null)
		{
			return Result<ChatResponse>.Failure(
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
			return Result<ChatResponse>.Failure(
				Error.NotFound("Unable to access this chat.")
			);
		}

		if (!chat.IsGroup)
		{
			return Result<ChatResponse>.Failure(
				Error.Validation("You can't remove members from a private chat.")
			);
		}

		var isAdmin = chat.ChatMembers.Any(cm =>
			cm.UserId == currentUserId &&
			cm.Role == ChatMemberRole.Admin
		);

		if (!isAdmin)
		{
			return Result<ChatResponse>.Failure(
				Error.Validation("You do not have permission to remove members from this chat.")
			);
		}

		var memberToRemove = chat.ChatMembers.FirstOrDefault(cm => cm.UserId == memberIdToRemove);
		if (memberToRemove is null)
		{
			return Result<ChatResponse>.Failure(
				Error.NotFound($"The user '{memberIdToRemove}' is not a member of this chat.")
			);
		}

		chat.ChatMembers.Remove(memberToRemove);

		await dbContext.SaveChangesAsync();

		return Result<ChatResponse>.Success(
			new ChatResponse(chat.Id, chat.Name, chat.IsGroup, chat.CreatedAt)
		);
	}

	public async Task<Result<bool>> LeaveChatAsync(string currentUserId, Guid chatId)
	{
		var chat = await dbContext.Chats
			.Include(cm => cm.ChatMembers)
			.FirstOrDefaultAsync(c => c.Id == chatId);

		if (chat is null)
		{
			return Result<bool>.Failure(
				Error.NotFound($"No chat was found with the provided ID: {chatId}")
			);
		}

		var member = chat.ChatMembers.FirstOrDefault(cm => cm.UserId == currentUserId);
		if (member is null)
		{
			return Result<bool>.Failure(
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

		return Result<bool>.Success(true);
	}

	public async Task<bool> IsChatMemberAsync(string currentUserId, Guid chatId)
	{
		return await dbContext.ChatMembers.
			AnyAsync(cm =>
				cm.ChatId == chatId &&
				cm.UserId == currentUserId
			);
	}
}
