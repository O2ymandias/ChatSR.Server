using ChatSR.Application.Interfaces;
using ChatSR.Application.Services;
using ChatSR.Application.Shared.Options;

namespace ChatSR.Api.Extensions;

public static class AppServicesExtensions
{
	public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IChatService, ChatService>();
		services.AddScoped<IMessageService, MessageService>();
		services.AddScoped<IConnectionManager, ConnectionManager>();
		services.AddScoped<IImageUploader, ImageUploader>();

		services.Configure<ImageUploaderOptions>(config.GetSection("ImageUploaderOptions"));

		return services;
	}
}
