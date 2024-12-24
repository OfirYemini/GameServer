using Google.Protobuf;

namespace GameServer.Core.Interfaces;

public interface INotificationManager
{
    event Func<int, IMessage, Task> OnMessageRecieved;
    Task SendMessageAsync(int targetPlayerId, IMessage message);
}