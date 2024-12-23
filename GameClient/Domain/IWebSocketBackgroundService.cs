using Game.Contracts;
using Google.Protobuf;

namespace GameClient.Domain;

public interface IWebSocketBackgroundService
{
    Task SendMessageAsync(MessageType messageType, IMessage message);
}