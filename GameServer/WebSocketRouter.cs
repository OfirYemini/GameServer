using System.Net.WebSockets;

namespace GameServer;

public class WebSocketRouter
{
    private readonly Dictionary<string, IWebSocketHandler> _handlers;

    public WebSocketRouter(IEnumerable<IWebSocketHandler> handlers)
    {
        _handlers = handlers.ToDictionary(k=>$"/ws/{k.Route}", v => v);
    }
    
    public async Task RouteAsync(HttpContext context, WebSocket webSocket)
    {
        if (context.Request.Path.HasValue && _handlers.TryGetValue(context.Request.Path.Value, out var handler))
        {
            await handler.HandleWebSocketAsync(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}