using System.Net.WebSockets;
using AutoMapper;
using Game.Server;
using Game.Server.DataAccess;
using GameServer.Application;
using GameServer.Application.Handlers;
using GameServer.Core.Interfaces;
using GameServer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using WebSocketManager = GameServer.Infrastructure.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);



builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

WebSocketOptions wsOptions = new WebSocketOptions();
builder.Configuration.GetSection("WebSockets").Bind(wsOptions);


var services = builder.Services;

services.AddDbContextFactory<GameDbContext>(options =>
{
    options.UseSqlite("Data Source=GameServerDb.db");
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

string redisConnectionString = builder.Configuration.GetConnectionString("Redis:ConnectionString") ?? "localhost:6379";
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));


services.AddSingleton<IHandler, LoginHandler>();
services.AddSingleton<IHandler, UpdateResourceHandler>();
services.AddSingleton<IHandler, SendGiftHandler>();
services.AddSingleton<INotificationManager, NotificationManager>();
services.AddSingleton<IGameRepository, GameRepository>();


services.AddSingleton<IWebSocketManager>(provider =>
{
    var handlers = provider.GetServices<IHandler>();
    var notificationManager = provider.GetRequiredService<INotificationManager>();
    var logger = provider.GetRequiredService<ILogger<WebSocketManager>>();
    return new WebSocketManager(handlers,notificationManager,logger);
});

var app = builder.Build();
app.UseWebSockets(wsOptions);

app.Use(async (HttpContext context,RequestDelegate next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    if (context.WebSockets.IsWebSocketRequest)
    {
        try
        {
            var webSocketManager = context.RequestServices.GetRequiredService<IWebSocketManager>();
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketManager.HandleWebSocketSessionAsync(context, webSocket);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to connect to web socket");
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

});

app.Run();