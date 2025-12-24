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

		await connectionManager.AddConnectionAsync(userId, Context.ConnectionId);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = GetCurrentUserId();
		if (userId is not null)
		{
			await connectionManager.RemoveConnectionAsync(userId, Context.ConnectionId);
		}

		await base.OnDisconnectedAsync(exception);
	}

	public async Task Heartbeat()
	{
		var userId = GetCurrentUserId();
		if (userId is null) return;

		await connectionManager.KeepAliveAsync(userId);

		Console.WriteLine("Heartbeat");
	}

	private string? GetCurrentUserId() => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
