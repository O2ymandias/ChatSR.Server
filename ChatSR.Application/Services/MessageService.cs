using ChatSR.Application.Dtos.MessageDtos;
using ChatSR.Application.Interfaces;
using ChatSR.Application.Shared.Constants;
using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;
using ChatSR.Infrastructure.Data;
using ChatSR.Infrastructure.Entities;
using ChatSR.Infrastructure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatSR.Application.Services;

public class MessageService(AppDbContext dbContext, IChatService chatService) : IMessageService
{
	private const int MaxMessageLength = 2000;

	public async Task<Result<MessageResponse>> SendMessageAsync(string currentUserId, Guid chatId, SendMessageRequest request)
	{
		var content = request.Content.Trim();

		if (string.IsNullOrWhiteSpace(content))
		{
			return Result<MessageResponse>.Failure(
				Error.Validation("Message content can't be empty.")
			);
		}

		if (content.Length > MaxMessageLength)
		{
			return Result<MessageResponse>.Failure(
				Error.Validation($"Message content can't exceed {MaxMessageLength}.")
			);
		}

		if (!await chatService.IsChatMemberAsync(currentUserId, chatId))
		{
			return Result<MessageResponse>.Failure(
				Error.Validation($"Unable to access this chat.")
			);
		}

		var newMessage = new Message()
		{
			Id = Guid.NewGuid(),
			ChatId = chatId,
			UserId = currentUserId,
			Content = content.Trim()
		};

		dbContext.Messages.Add(newMessage);

		await dbContext.SaveChangesAsync();

		var sender = await dbContext.Users.FindAsync(currentUserId);

		return Result<MessageResponse>.Success(
			new MessageResponse(
				newMessage.Id,
				newMessage.ChatId,
				newMessage.Content,
				newMessage.SentAt,
				false,
				null,
				sender?.Id ?? string.Empty,
				sender?.DisplayName ?? string.Empty,
				sender?.PictureUrl ?? string.Empty
			)
		);
	}

	public async Task<PagedResult<MessageResponse>> GetChatMessagesAsync(string currentUserId, Guid chatId, int page, int pageSize)
	{
		if (page <= 0)
			page = PaginationConstants.DefaultPage;

		if (pageSize <= 0)
			pageSize = PaginationConstants.DefaultPageSize;

		if (pageSize > PaginationConstants.MaxPageSize)
			pageSize = PaginationConstants.MaxPageSize;


		if (!await chatService.IsChatMemberAsync(currentUserId, chatId))
		{
			return PagedResult<MessageResponse>.Failure(
				Error.Validation($"Unable to access this chat.")
			);
		}

		var totalCount = await dbContext.Messages
			.Where(m => m.ChatId == chatId)
			.CountAsync();

		if (totalCount == 0)
		{
			return PagedResult<MessageResponse>.Success(
				[],
				new PaginationMetadata(page, pageSize, totalCount)
			);
		}

		var messages = await dbContext.Messages
			.Where(m => m.ChatId == chatId)
			.OrderByDescending(m => m.SentAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(m => new MessageResponse(
				m.Id,
				m.ChatId,
				m.Content,
				m.SentAt,
				m.IsEdited,
				m.EditedAt,
				m.UserId,
				m.User.DisplayName,
				m.User.PictureUrl ?? string.Empty
			))
			.ToListAsync();


		return PagedResult<MessageResponse>.Success(
			messages,
			new PaginationMetadata(page, pageSize, totalCount)
		);
	}

	public async Task<Result<MessageResponse>> EditMessageAsync(string currentUserId, Guid messageId, EditMessageRequest request)
	{
		var newContent = request.NewContent.Trim();

		if (string.IsNullOrWhiteSpace(newContent))
		{
			return Result<MessageResponse>.Failure(
				Error.Validation("Message content can't be empty.")
			);
		}

		if (newContent.Length > MaxMessageLength)
		{
			return Result<MessageResponse>.Failure(
				Error.Validation($"Message content can't exceed {MaxMessageLength}.")
			);
		}

		var message = await dbContext.Messages
			.Include(u => u.User)
			.FirstOrDefaultAsync(m =>
				m.Id == messageId &&
				m.UserId == currentUserId
			);

		if (message is null)
		{
			return Result<MessageResponse>.Failure(
				Error.NotFound($"Message not found or you don't have permission to edit it.")
			);
		}

		if (message.Content == newContent)
		{
			return Result<MessageResponse>.Failure(
				Error.Validation($"No changes were found.")
			);
		}

		message.Content = newContent;
		message.IsEdited = true;
		message.EditedAt = DateTime.UtcNow;

		await dbContext.SaveChangesAsync();

		return Result<MessageResponse>.Success(
			new MessageResponse(
				message.Id,
				message.ChatId,
				message.Content,
				message.SentAt,
				message.IsEdited,
				message.EditedAt,
				message.User.Id,
				message.User.DisplayName,
				message.User.PictureUrl
			)
		);
	}

	public async Task<Result<bool>> DeleteMessageAsync(string currentUserId, Guid messageId)
	{
		var message = await dbContext.Messages
			.Include(m => m.Chat)
				.ThenInclude(c => c.ChatMembers)
			.FirstOrDefaultAsync(m => m.Id == messageId);

		if (message is null)
		{
			return Result<bool>.Failure(
				Error.NotFound("Message not found.")
			);
		}

		var isMessageSender = message.UserId == currentUserId;
		var isAdmin = message.Chat.ChatMembers.Any(cm =>
			cm.UserId == currentUserId &&
			cm.Role == ChatMemberRole.Admin
		);

		if (!isMessageSender)
		{
			if (!isAdmin)
			{
				return Result<bool>.Failure(
					Error.Validation("You can only delete your own messages or be a chat admin.")
				);
			}
		}

		dbContext.Messages.Remove(message);

		await dbContext.SaveChangesAsync();

		return Result<bool>.Success(true);
	}
}
