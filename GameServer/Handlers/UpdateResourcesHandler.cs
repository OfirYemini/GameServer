using System.Net.WebSockets;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class UpdateResourcesHandler:IWebSocketHandler
{
    public MessageType MessageType { get; } = MessageType.UpdateRequest;
    public MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourcesHandler()
    {
    }
    
    public Task<IMessage> HandleMessageAsync(MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}