using System.Net.WebSockets;

namespace GameServer;

public interface IWebSocketHandler
{
    string Route { get; }
    Task HandleAsync(HttpContext context, WebSocket webSocket);
}

public class WebSocketRouter
{
    private readonly Dictionary<string, IWebSocketHandler> _handlers;

    public WebSocketRouter(IEnumerable<IWebSocketHandler> handlers)
    {
        _handlers = handlers.ToDictionary(k=>k.Route, v => v);
    }
    
    public async Task RouteAsync(HttpContext context, WebSocket webSocket)
    {
        if (!context.Request.Path.HasValue)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        
        var path = new Uri(context.Request.Path.Value);
        var routeSegment =path.Segments[^1]; 
        if (_handlers.TryGetValue(routeSegment, out var handler))
        {
            await handler.HandleAsync(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}