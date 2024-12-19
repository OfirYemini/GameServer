using System.Net.WebSockets;

namespace GameServer.Handlers;

public class LoginHandler:IWebSocketHandler
{
    public string Route { get; } = "login";
    public Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        throw new NotImplementedException();
    }
}