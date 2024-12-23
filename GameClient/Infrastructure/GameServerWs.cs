using System.Net.WebSockets;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;

namespace GameClient.Infrastructure;

using Game.Contracts;
using GameClient.Domain;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
public class GameServerWs : IGameServerWs,IDisposable
{
    public event Action<ServerResponse>? OnMessageReceived;
    private readonly IConfiguration _config;
    private readonly ILogger<GameServerWs> _logger;
    private readonly ClientWebSocket _webSocket;
    private readonly CancellationTokenSource _cts;
    private Task _receiveLoopTask;

    public GameServerWs(IConfiguration config,ILogger<GameServerWs> logger)
    {
        _config = config;
        _logger = logger;
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        StartReceiveLoop();
    }
    
    private void StartReceiveLoop()
    {
        _receiveLoopTask = Task.Run(async () =>
        {
            try
            {
                var wsUrl = _config["AppSettings:WebSocketUrl"];
                if (string.IsNullOrEmpty(wsUrl))
                {
                    _logger.LogWarning("No 'AppSettings:WebSocketUrl' found in config. Skipping WebSocket connection.");
                    return;
                }

                _logger.LogInformation("Connecting to WebSocket at {Url}", wsUrl);
                await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);
                _logger.LogInformation("WebSocket connected (State={State}).", _webSocket.State);

                var buffer = new byte[1024 * 4];
                while (!_cts.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("Server requested close. Closing WebSocket...");
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token);
                    }
                    else
                    {
                        using var stream = new MemoryStream(buffer, 0, result.Count);
                        ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(stream);
                        DispatchMessage(serverResponse);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when _cts is canceled
                _logger.LogInformation("WebSocket receive loop canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket connection or receive error.");
            }
        }, _cts.Token);
    }

    public async Task LoginAsync(string deviceId)
    {
        var message = new LoginRequest()
        {
            DeviceId = deviceId,
        };
        await SendMessageAsync(MessageType.LoginRequest, message);
    }

    public async Task UpdateResourceAsync(ResourceType resourceType, int resourceValue)
    {
        var message = new UpdateRequest()
        {
            ResourceType = resourceType,
            ResourceValue = resourceValue
        };
        await SendMessageAsync(MessageType.UpdateRequest, message);
    }

    public async Task SendGiftAsync(int toPlayerId,ResourceType resourceType, int resourceValue)
    {
        var message = new SendGiftRequest()
        {
            FriendPlayerId = toPlayerId,
            ResourceType = resourceType,
            ResourceValue = resourceValue
        };
        await SendMessageAsync(MessageType.SendGift, message);
    }

    public async Task SendMessageAsync(MessageType messageType, IMessage message)
    {
        using var stream = new MemoryStream();
        
        stream.WriteByte((byte)messageType);
        
        message.WriteTo(stream);

        byte[] bytes = stream.ToArray();

        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None
        );

    }
    
    private void DispatchMessage(ServerResponse serverResponse)
    {
        if (OnMessageReceived != null)
        {
            OnMessageRecieved(serverResponse);
        }
    }
    
    public void Dispose()
    {
        _webSocket.Dispose();
        _cts.Dispose();
        _receiveLoopTask.Dispose();
    }
}
