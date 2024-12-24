using System.Net.WebSockets;
using Game.Contracts;
using Game.Server.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Game.Server;

// public interface IWebSocketHandler<TRequest, TResponse>
// {
//     string Route { get; }
//     Task<TResponse> HandleAsync(TRequest request);
// }

public interface IWebSocketHandler
{
    MessageType MessageType { get; }
    
    Task<IMessage> HandleMessageAsync(PlayerInfo info,MemoryStream stream);
}