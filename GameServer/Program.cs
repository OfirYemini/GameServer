using System.Net.WebSockets;
using GameServer;
using GameServer.Common;
using GameServer.DataAccess;
using GameServer.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


WebSocketOptions wsOptions = new WebSocketOptions()
{
    //KeepAliveInterval = TimeSpan.FromMinutes(1), // todo: default is 2, load from config
    //AllowedOrigins = { "https://localhost:4201" } // todo: use certificate
    
};

var services = builder.Services;

services.AddDbContextFactory<GameDbContext>(options =>
    options.UseInMemoryDatabase("GameServerDb"));

services.AddSingleton<IWebSocketHandler, LoginHandler>();
services.AddSingleton<IWebSocketHandler, UpdateResourcesHandler>();
services.AddSingleton<ISessionManager, SessionManager>();
services.AddSingleton<IGameRepository, GameRepository>();
services.AddSingleton<IWebSocketMessageSerializer, WebSocketMessageSerializer>();

services.AddSingleton<WebSocketRouter>(provider =>
{
    var handlers = provider.GetServices<IWebSocketHandler>();
    return new WebSocketRouter(handlers);
});

var app = builder.Build();
app.UseWebSockets(wsOptions);

app.Use(async (HttpContext context,RequestDelegate next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocketRouter = context.RequestServices.GetRequiredService<WebSocketRouter>();
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();//todo: should I use var cancellationToken = context.RequestAborted;
        var deviceId = context.Request.Query["deviceId"];
        try
        {
            
            await webSocketRouter.RouteAsync(context, webSocket);    
            
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