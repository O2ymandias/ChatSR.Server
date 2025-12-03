using ChatSR.Application.Contracts;
using ChatSR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatSR.Api.Extensions;

public static class AppInitializerExtensions
{
	public async static Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var serviceProvider = scope.ServiceProvider;

		var appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
		await appDbContext.Database.MigrateAsync();

		var databaseSeeder = serviceProvider.GetRequiredService<IDatabaseSeeder>();
		await databaseSeeder.SeedRolesAsync();

		return app;
	}
}
