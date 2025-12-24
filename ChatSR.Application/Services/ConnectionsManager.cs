using ChatSR.Application.Interfaces;
using StackExchange.Redis;

namespace ChatSR.Application.Services;

public class ConnectionManager(IConnectionMultiplexer redis) : IConnectionManager
{
	private readonly IDatabase _db = redis.GetDatabase();
	private const int MinutesToExpire = 2;
	private const string ConnectionKeyPrefix = "user:connections:";

	public async Task<List<string>> GetConnectionsAsync(string userId)
	{
		var key = GetRedisKey(userId);
		var connections = await _db.SetMembersAsync(key);
		return [.. connections.Select(c => c.ToString())];
	}

	public async Task AddConnectionAsync(string userId, string connectionId)
	{
		var key = GetRedisKey(userId);
		await _db.SetAddAsync(key, connectionId);
		await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(MinutesToExpire));
	}


	public async Task RemoveConnectionAsync(string userId, string connectionId)
	{
		var key = GetRedisKey(userId);
		await _db.SetRemoveAsync(key, connectionId);

		var remainingConnections = await _db.SetLengthAsync(key);
		if (remainingConnections == 0)
		{
			await _db.KeyDeleteAsync(key);
		}
	}

	public async Task<bool> IsUserOnlineAsync(string userId)
	{
		var key = GetRedisKey(userId);
		var remainingConnections = await _db.SetLengthAsync(key);
		return remainingConnections > 0;
	}

	public async Task KeepAliveAsync(string userId)
	{
		var key = GetRedisKey(userId);
		await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(MinutesToExpire));
	}

	private static string GetRedisKey(string userId) => $"{ConnectionKeyPrefix}{userId}";

}
