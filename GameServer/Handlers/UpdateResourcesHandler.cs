using System.Net.WebSockets;

namespace GameServer.Handlers;

public class UpdateResourcesHandler:IWebSocketHandler
{
    public string Route { get; } = "update";
    public Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        throw new NotImplementedException();
    }
}