using ChatSR.Application.Interfaces;
using ChatSR.Application.Services;
using ChatSR.Application.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChatSR.Api.Extensions;

public static class AuthServicesExtensions
{
	public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration config)
	{
		services.Configure<JwtOptions>(config.GetSection("JwtOptions"));
		services.AddScoped<ITokenService, TokenService>();

		services
			.AddAuthentication(opts =>
			{
				opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(opts =>
			{
				var jwtOptions = config.GetSection("JwtOptions").Get<JwtOptions>()
				?? throw new InvalidOperationException("Failed to parse JwtOptions.");

				opts.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,

					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,

					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				};

				// SignalR can't send Authorization headers when using WebSockets.
				// Instead, it sends the JWT access token as a query string parameter (?access_token=...).
				// This event extracts token from the query string and attaches it to the authentication context.
				opts.Events = new JwtBearerEvents()
				{
					// This event runs for every incoming request before authentication happens
					OnMessageReceived = context =>
					{
						var accessToken = context.Request.Query["access_token"];

						// If the request is for hubs
						var path = context.HttpContext.Request.Path;
						if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs/chat")))
						{
							// Read the token out of the query string
							context.Token = accessToken;
						}
						return Task.CompletedTask;
					}
				};
			});

		return services;
	}
}
