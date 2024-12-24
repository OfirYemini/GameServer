using Game.Contracts;
using Game.Server.Common;
using GameServer.Core.Entities;
using Google.Protobuf;

namespace GameServer.Core.Interfaces;

public interface ICommandHandler
{
    MessageType MessageType { get; }
    
    Task<IMessage> HandleMessageAsync(PlayerInfo info,MemoryStream stream);
}