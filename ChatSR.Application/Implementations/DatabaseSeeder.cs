using ChatSR.Application.Contracts;
using ChatSR.Application.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChatSR.Application.Implementations;

public class DatabaseSeeder(RoleManager<IdentityRole> roleManager, ILogger<DatabaseSeeder> logger) : IDatabaseSeeder
{
	public async Task SeedRolesAsync()
	{
		if (roleManager.Roles.Any()) return;

		string[] appRoles = [RoleConstants.Admin, RoleConstants.User];

		foreach (var role in appRoles)
		{
			var creationResult = await roleManager.CreateAsync(new IdentityRole(role));
			if (!creationResult.Succeeded)
			{
				var errors = string.Join(", ", creationResult.Errors.Select(e => e.Description));
				logger.LogError("Unable to create role '{Role}' due to: {Errors}", role, errors);
			}
			logger.LogInformation("Role '{Role}' created successfully.", role);
		}
	}
}
