using System.Net.WebSockets;

namespace GameServer.Application.Interfaces;

public interface IWebSocketManager
{
    Task HandleWebSocketSessionAsync(HttpContext context, WebSocket webSocket);
}