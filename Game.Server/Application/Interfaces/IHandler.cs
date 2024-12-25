using Game.Contracts;
using GameServer.Core.Entities;
using Google.Protobuf;

namespace GameServer.Application.Interfaces;

public interface IHandler
{
    MessageType MessageType { get; }
    
    Task<IMessage> HandleMessageAsync(PlayerInfo playerInfo,MemoryStream stream);
}