using Game.Contracts;
using GameServer.Core.Entities;
using Google.Protobuf;

namespace GameServer.Core.Interfaces;

public interface IHandler
{
    MessageType MessageType { get; }
    
    Task<IMessage> HandleMessageAsync(PlayerInfo playerInfo,MemoryStream stream);
}