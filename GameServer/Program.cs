using GameServer;
using GameServer.Handlers;

var builder = WebApplication.CreateBuilder(args);


WebSocketOptions wsOptions = new WebSocketOptions()
{
    //KeepAliveInterval = TimeSpan.FromMinutes(1), // todo: default is 2, load from config
    //AllowedOrigins = { "https://localhost:4201" } // todo: use certificate
    
};

var services = builder.Services;

services.AddSingleton<IWebSocketHandler, LoginHandler>();
services.AddSingleton<IWebSocketHandler, UpdateResourcesHandler>();

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
        await webSocketRouter.RouteAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }

});

app.Run();