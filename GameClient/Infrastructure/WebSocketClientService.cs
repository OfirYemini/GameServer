using System.Net.WebSockets;
using System.Text;
using Game.Contracts;
using GameClient.Domain;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameClient.Infrastructure;

public class WebSocketBackgroundService : IWebSocketBackgroundService ,IDisposable
{
    private readonly IConfiguration _config;
    private readonly ILogger<WebSocketBackgroundService> _logger;
    private ClientWebSocket _webSocket;
    private readonly CancellationTokenSource _cts;
    private Task _receiveLoopTask;

    public WebSocketBackgroundService(IConfiguration config, ILogger<WebSocketBackgroundService> logger)
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
                        PrintMessage(stream);
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

    private void PrintMessage(MemoryStream stream)
    {
         ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(stream);
         
         // Deserialize based on expected response type
         if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.LoginResponse)
         {
             var response = serverResponse.LoginResponse;
             Console.WriteLine($"LoginResponse: PlayerId={response.PlayerId}");
         }
         else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.UpdateResponse)
         {
             var response = serverResponse.UpdateResponse;
             Console.WriteLine($"UpdateResponse: New Balance={response.NewBalance}");
         }
         else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.ServerError)
         {
             var response = serverResponse.ServerError;
             Console.WriteLine($"Unexpected error received. {response.Message}");
         }
         else
         {
             Console.WriteLine("Unexpected response received.");
         }
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

    public void Dispose()
    {
        _webSocket.Dispose();
        _cts.Dispose();
        _receiveLoopTask.Dispose();
    }
}

