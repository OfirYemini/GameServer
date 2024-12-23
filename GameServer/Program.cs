using System.Net.WebSockets;
using GameServer;
using GameServer.Common;
using GameServer.DataAccess;
using GameServer.Handlers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebSocketManager = GameServer.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);


WebSocketOptions wsOptions = new WebSocketOptions()
{
    //KeepAliveInterval = TimeSpan.FromMinutes(1), // todo: default is 2, load from config
    //AllowedOrigins = { "https://localhost:4201" } // todo: use certificate
    
};

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var services = builder.Services;

services.AddDbContextFactory<GameDbContext>(options =>
    options.UseInMemoryDatabase("GameServerDb"));

services.AddSingleton<IWebSocketHandler, LoginHandler>();
services.AddSingleton<IWebSocketHandler, UpdateResourcesHandler>();
services.AddSingleton<INotificationManager, NotificationManager>();
services.AddSingleton<IGameRepository, GameRepository>();
services.AddSingleton<IWebSocketMessageSerializer, WebSocketMessageSerializer>();

services.AddSingleton<WebSocketManager>(provider =>
{
    var handlers = provider.GetServices<IWebSocketHandler>();
    var notificationManager = provider.GetRequiredService<INotificationManager>();
    var logger = provider.GetRequiredService<ILogger<WebSocketManager>>();
    return new WebSocketManager(handlers,notificationManager,logger);
});

var app = builder.Build();
app.UseWebSockets(wsOptions);

app.Use(async (HttpContext context,RequestDelegate next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocketRouter = context.RequestServices.GetRequiredService<WebSocketManager>();
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();//todo: should I use var cancellationToken = context.RequestAborted;
        var deviceId = context.Request.Query["deviceId"];
        try
        {
            
            await webSocketRouter.HandleWebSocketSessionAsync(context, webSocket);    
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            // if (!string.IsNullOrEmpty(deviceId))
            // {
            //     var sessionManager = context.RequestServices.GetRequiredService<ISessionManager>();
            //     await sessionManager.TryRemoveAsync(new Guid(deviceId));
            //     
            //     Console.WriteLine($"Player with DeviceId {deviceId} disconnected.");
            // }
            //
            //
            // if (webSocket.State != WebSocketState.Closed)
            // {
            //     await webSocket.CloseAsync(
            //         WebSocketCloseStatus.NormalClosure,
            //         "Closed by server",
            //         CancellationToken.None
            //     );
            // }
        }
        
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

});

app.Run();