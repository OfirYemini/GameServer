using System.Net.WebSockets;
using Google.Protobuf.Reflection;

namespace GameServer;

// public interface IWebSocketHandler<TRequest, TResponse>
// {
//     string Route { get; }
//     Task<TResponse> HandleAsync(TRequest request);
// }

public interface IWebSocketHandler
{
    Request.InnerMessageOneofCase MessageDescriptor { get; }
    Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket);
}