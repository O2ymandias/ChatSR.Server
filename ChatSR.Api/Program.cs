using ChatSR.Api.Extensions;
using ChatSR.Api.Hubs;
using ChatSR.Infrastructure.Data;
using ChatSR.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(opts =>
{
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	opts.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
	var redisConnection = builder.Configuration.GetConnectionString("RedisConnection") ?? throw new Exception("Redis connection string is missing.");
	var config = ConfigurationOptions.Parse(redisConnection, true);
	config.AbortOnConnectFail = false;
	return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddSignalR();

builder.Services
	.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAppServices();
builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddCors(opts =>
{
	opts.AddPolicy("ChatSR.Client", policy =>
	{
		policy
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials()
		.WithOrigins(
			builder.Configuration["ClientUrl"]
			?? throw new Exception("Something went wrong while trying to parse the 'ClientUrl'.")
		);
	});
});

#endregion

var app = builder.Build();

await app.InitializeDatabaseAsync();

#region  Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("ChatSR.Client");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

#endregion

app.Run();