using System.Threading.Channels;
using Game.Server.Handlers;
using Game.Server.Common;
using GameServer.Application.Commands;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace Game.Server;

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