namespace ChatSR.Application.Interfaces;

public interface IConnectionManager
{
	Task<List<string>> GetConnectionsAsync(string userId);
	Task AddConnectionAsync(string userId, string connectionId);
	Task RemoveConnectionAsync(string userId, string connectionId);
	Task<bool> IsUserOnlineAsync(string userId);
	Task KeepAliveAsync(string userId);
}
