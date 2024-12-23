using System.Threading.Channels;
using GameServer.Common;
using GameServer.Handlers;
using Google.Protobuf;

namespace GameServer;

public class NotificationManager:INotificationManager
{
    private readonly Channel<(int targetId, IMessage message)> _channel;

    public NotificationManager(IConfiguration configuration)
    {
        var capacity = configuration.GetValue("NotificationManager:BoundedChannelOptions:Capacity", 100);
        _channel = Channel.CreateBounded<(int targetId, IMessage message)>(new BoundedChannelOptions(capacity));
    }

    public async Task SendMessageAsync(int targetPlayerId, IMessage message)
    {
        await _channel.Writer.WriteAsync((targetPlayerId, message));
    }
}