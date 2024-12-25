using Google.Protobuf;

namespace GameServer.Application.Interfaces;

public interface INotificationManager
{
    event Func<int, IMessage, Task> OnMessageRecieved;
    Task SendMessageAsync(int targetPlayerId, IMessage message);
}