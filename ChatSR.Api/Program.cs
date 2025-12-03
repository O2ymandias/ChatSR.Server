using ChatSR.Api.Extensions;
using ChatSR.Infrastructure.Data;
using ChatSR.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(opts =>
{
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	opts.UseSqlServer(connectionString);
});

builder.Services
	.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAppServices();
builder.Services.AddAuthServices(builder.Configuration);

#endregion

var app = builder.Build();

await app.InitializeDatabaseAsync();

#region  Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();