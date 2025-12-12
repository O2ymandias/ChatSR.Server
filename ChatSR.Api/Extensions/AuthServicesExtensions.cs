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
			});

		return services;
	}
}
