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

		var allConnections = new List<string>();

		foreach (var memberId in memberIds)
		{
			var connections = await connectionManager.GetConnectionsAsync(memberId);
			allConnections.AddRange(connections);
		}

		await Clients.Clients(allConnections)
			 .SendAsync("ReceiveMessage", result.Value);
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
