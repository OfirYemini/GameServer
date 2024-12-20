using System.Net.WebSockets;

namespace GameServer;

// public interface IWebSocketHandler<TRequest, TResponse>
// {
//     string Route { get; }
//     Task<TResponse> HandleAsync(TRequest request);
// }

public interface IWebSocketHandler
{
    string Route { get; }
    Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket);
}