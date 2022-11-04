using FollowTheLeader.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("null",
                                "https://localhost:7032");
            builder.AllowAnyHeader();
        });
});
builder.Services.AddSignalR();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors();

app.MapHub<GameHub>("/Game");

app.Run();
