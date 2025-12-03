using ChatSR.Application.Contracts;
using ChatSR.Application.Implementations;

namespace ChatSR.Api.Extensions;

public static class AppServicesExtensions
{
	public static IServiceCollection AddAppServices(this IServiceCollection services)
	{
		services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
		services.AddScoped<IAuthService, AuthService>();
		return services;
	}
}
