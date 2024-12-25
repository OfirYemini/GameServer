using GameServer.Core.Interfaces;
using Google.Protobuf;
using StackExchange.Redis;

namespace GameServer.Infrastructure;

public class NotificationManager : INotificationManager, IDisposable
{
    private const string ChannelName = "PlayerNotifications";
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<NotificationManager> _logger;
    private ChannelMessageQueue? _subscriptionQueue;
    public event Func<int, IMessage, Task>? OnMessageRecieved;

    public NotificationManager(IConnectionMultiplexer redisConnection,ILogger<NotificationManager> logger)
    {
        _redisConnection = redisConnection;
        _logger = logger;
        Subscribe();
    }

    private void Subscribe()
    {
        var subscriber = _redisConnection.GetSubscriber();
        _subscriptionQueue = subscriber.Subscribe(RedisChannel.Literal(ChannelName));
        _subscriptionQueue.OnMessage(async channelMessage =>
        {
            try
            {
                var notification = PubMessage.Parser.ParseFrom(channelMessage.Message);
                int targetPlayerId = notification.PlayerId;

                if (OnMessageRecieved != null)
                {
                    var msg = ServerResponse.Parser.ParseFrom(notification.Message);
                    await OnMessageRecieved(targetPlayerId, msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"OnMessage Error while processing message");
            }
        });
    }

    public async Task SendMessageAsync(int targetPlayerId,IMessage message)
    {
        try
        {
            var publisher = _redisConnection.GetSubscriber();
            var msg = new PubMessage
            {
                PlayerId = targetPlayerId,
                Message = message.ToByteString()
            };

            var serializedMessage = msg.ToByteArray();
            await publisher.PublishAsync(RedisChannel.Literal(ChannelName), serializedMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"SendMessageAsync Error while processing message");
        }
    }

    public void Dispose()
    {
        _subscriptionQueue?.Unsubscribe();
    }
}
