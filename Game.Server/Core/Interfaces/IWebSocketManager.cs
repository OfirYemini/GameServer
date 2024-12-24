using System.Net.WebSockets;

namespace GameServer.Core.Interfaces;

public interface IWebSocketManager
{
    Task HandleWebSocketSessionAsync(HttpContext context, WebSocket webSocket);
}