using System.Net.WebSockets;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer;

// public interface IWebSocketHandler<TRequest, TResponse>
// {
//     string Route { get; }
//     Task<TResponse> HandleAsync(TRequest request);
// }

public interface IWebSocketHandler
{
    MessageType MessageType { get; }
    
    Task<IMessage> HandleMessageAsync(PlayerSession session,MemoryStream stream);
}