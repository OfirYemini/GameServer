using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Infrastructure;

public class NotificationManager:INotificationManager
{
    public event Func<int, IMessage, Task>? OnMessageRecieved;

    public NotificationManager(IConfiguration configuration)
    {
        //var capacity = configuration.GetValue("NotificationManager:BoundedChannelOptions:Capacity", 100);
        //_channel = Channel.CreateBounded<(int targetId, IMessage message)>(new BoundedChannelOptions(capacity));
    }

    public async Task SendMessageAsync(int targetPlayerId, IMessage message)
    {
        if (OnMessageRecieved != null)
        {
            await OnMessageRecieved(targetPlayerId, message);
        }
    }
}