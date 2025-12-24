using ChatSR.Application.Interfaces;
using ChatSR.Application.Services;

namespace ChatSR.Api.Extensions;

public static class AppServicesExtensions
{
	public static IServiceCollection AddAppServices(this IServiceCollection services)
	{
		services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IChatService, ChatService>();
		services.AddScoped<IMessageService, MessageService>();
		services.AddScoped<IConnectionManager, ConnectionManager>();
		return services;
	}
}
