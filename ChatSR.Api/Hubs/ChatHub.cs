using ChatSR.Application.Dtos.MessageDtos;
using ChatSR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatSR.Api.Hubs;

[Authorize]
public class ChatHub(
	IChatService chatService,
	IMessageService messageService,
	IConnectionManager connectionManager
) : Hub
{
	public async override Task OnConnectedAsync()
	{
		await base.OnConnectedAsync();

		var userId = GetCurrentUserId();
		if (userId is null)
		{
			Context.Abort();
			return;
		}

		var wasOffline = !await connectionManager.IsUserOnlineAsync(userId);

		await connectionManager.AddConnectionAsync(userId, Context.ConnectionId);

		if (wasOffline)
			await NotifyUserStatusAsync("UserOnline", userId);

		await NotifyCallerOfOnlineUsersAsync(userId);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = GetCurrentUserId();
		if (userId is not null)
		{
			await connectionManager.RemoveConnectionAsync(userId, Context.ConnectionId);

			var isNowOffline = !await connectionManager.IsUserOnlineAsync(userId);

			if (isNowOffline)
				await NotifyUserStatusAsync("UserOffline", userId, DateTimeOffset.UtcNow);
		}

		await base.OnDisconnectedAsync(exception);
	}

	public async Task Heartbeat()
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		await connectionManager.KeepAliveAsync(userId);
	}

	public async Task SendMessage(Guid chatId, SendMessageRequest request)
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		if (string.IsNullOrWhiteSpace(request.Content))
		{
			await Clients.Caller.SendAsync("MessageError", "Message content can't be empty.");
			return;
		}

		var result = await messageService.SendMessageAsync(userId, chatId, request);

		if (!result.IsSuccess)
		{
			await Clients.Caller.SendAsync("MessageError", result.Error);
			return;
		}

		var memberIds = await chatService.GetChatMemberIdsAsync(chatId);

		var connectionsToNotify = new List<string>();

		foreach (var memberId in memberIds)
		{
			var connections = await connectionManager.GetConnectionsAsync(memberId);
			connectionsToNotify.AddRange(connections);
		}

		if (connectionsToNotify.Count == 0) return;

		// Fire 'ReceiveMessage' event to all chat members connections even the caller.
		await Clients.Clients(connectionsToNotify)
			 .SendAsync("ReceiveMessage", result.Value);
	}

	public async Task EditMessage(Guid messageId, EditMessageRequest request)
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		var result = await messageService.EditMessageAsync(userId, messageId, request);

		if (!result.IsSuccess)
		{
			await Clients.Caller.SendAsync("MessageError", result.Error);
			return;
		}

		var connectionsToNotify = new List<string>();

		var chatMemberIds = await chatService.GetChatMemberIdsAsync(result.Value!.ChatId);

		foreach (var chatMemberId in chatMemberIds)
		{
			var connections = await connectionManager.GetConnectionsAsync(chatMemberId);
			connectionsToNotify.AddRange(connections);
		}

		if (connectionsToNotify.Count == 0) return;

		// Fire 'MessageEdited' event to all members in the chat.
		await Clients
			.Clients(connectionsToNotify)
			.SendAsync("MessageEdited", result.Value);

	}

	public async Task StartTyping(Guid chatId)
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;


		var chatMemberIds = await chatService.GetChatMemberIdsAsync(chatId);

		foreach (var chatMemberId in chatMemberIds)
		{
			if (userId == chatMemberId) continue;

			var connectionIds = await connectionManager.GetConnectionsAsync(chatMemberId);

			if (connectionIds.Count > 0)
			{
				await Clients
					.Clients(connectionIds)
					.SendAsync("UserTyping", chatId, userId);
			}
		}
	}

	public async Task StopTyping(Guid chatId)
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		var chatMemberIds = await chatService.GetChatMemberIdsAsync(chatId);

		foreach (var chatMemberId in chatMemberIds)
		{
			if (userId == chatMemberId) continue;

			var connectionIds = await connectionManager.GetConnectionsAsync(chatMemberId);

			if (connectionIds.Count > 0)
			{
				await Clients
					.Clients(connectionIds)
					.SendAsync("UserStoppedTyping", chatId, userId);
			}
		}
	}

	public async Task MarkChatAsRead(Guid chatId)
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		var lastReadAt = await chatService.MarkChatAsReadAsync(userId, chatId);
		if (lastReadAt is null) return;

		List<string> connectionsToNotify = [];

		var chatMemberIds = await chatService.GetChatMemberIdsAsync(chatId);
		foreach (var chatMemberId in chatMemberIds)
		{
			if (userId == chatMemberId) continue; // Exclude the current user

			var connections = await connectionManager.GetConnectionsAsync(chatMemberId);
			connectionsToNotify.AddRange(connections);
		}

		if (connectionsToNotify.Count == 0) return;

		// Fire 'ChatRead' event to all the chat members connections providing:
		// 1. ChatId: The chat that has been read
		// 2. UserId: The user that has read the chat
		// 3. LastReadAt: The time that this chat has been read by this user
		await Clients
			.Clients(connectionsToNotify)
			.SendAsync("ChatRead", chatId, userId, lastReadAt.Value);

	}


	private string? GetCurrentUserId() => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

	private async Task NotifyUserStatusAsync(string status, string userId, params object[] args)
	{
		var sharedMemberIds = await chatService.GetSharedMemberIdsAsync(userId);

		if (sharedMemberIds.Count == 0) return;

		List<string> connections = [];

		foreach (var memberId in sharedMemberIds)
			connections.AddRange(await connectionManager.GetConnectionsAsync(memberId));

		if (connections.Count == 0) return;

		await Clients
			.Clients(connections)
			.SendAsync(status, userId, args);
	}

	private async Task NotifyCallerOfOnlineUsersAsync(string userId)
	{
		var sharedMemberIds = await chatService.GetSharedMemberIdsAsync(userId);

		List<string> onlineSharedMembers = [];

		foreach (var memberId in sharedMemberIds)
		{
			if (await connectionManager.IsUserOnlineAsync(memberId))
				onlineSharedMembers.Add(memberId);
		}

		if (onlineSharedMembers.Count == 0) return;

		await Clients.Caller.SendAsync("OnlineUsers", onlineSharedMembers);
	}
}
