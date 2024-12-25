using System.Collections.Concurrent;
using System.Net.WebSockets;
using Game.Contracts;
using GameServer.Core.Interfaces;
using Google.Protobuf;
using PlayerInfo = GameServer.Core.Entities.PlayerInfo;

namespace GameServer.Infrastructure;

public class WebSocketManager : IWebSocketManager,IDisposable
{
    private readonly ILogger<WebSocketManager> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly Dictionary<MessageType, IHandler> _handlers;
    private readonly ConcurrentDictionary<int, (WebSocket webSocket, PlayerInfo playerInfo)> _activeSessions = new();

    public WebSocketManager(IEnumerable<IHandler> handlers, INotificationManager notificationManager, ILogger<WebSocketManager> logger)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _handlers = handlers.ToDictionary(h => h.MessageType, h => h);
        
        _notificationManager.OnMessageRecieved += HandleIncomingMessageAsync;
    }

    private async Task HandleIncomingMessageAsync(int targetId, IMessage message)
    {
        if (_activeSessions.TryGetValue(targetId, out var session) && session.webSocket.State == WebSocketState.Open)
        {
            await SendMessageAsync(session.webSocket, message);
        }
        else
        {
            _logger.LogInformation("No active session for {TargetId}, message discarded", targetId);
        }
    }

    public async Task HandleWebSocketSessionAsync(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[4096];
        var connectionId = context.Connection.Id;
        _logger.LogInformation("WebSocket connection established: {ConnectionId}", connectionId);

        PlayerInfo? playerInfo = null;
        
        while (webSocket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket {ConnectionId} closing", connectionId);
                    break;
                }

                using var inputStream = new MemoryStream(buffer, 0, result.Count);
                var messageType = (MessageType)inputStream.ReadByte();

                if (playerInfo == null || messageType == MessageType.LoginRequest)
                {
                    playerInfo = await ProcessLoginRequestAsync(webSocket, messageType, connectionId, inputStream);
                    continue;
                }

                await ProcessMessageAsync(playerInfo, messageType, inputStream, connectionId, webSocket);
            }
            catch (WebSocketException wse)
            {
                _logger.LogError(wse, "WebSocket error for {ConnectionId}", connectionId);
                break;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateServerError("An unexpected error occurred");
                _logger.LogError(ex, "Error processing message for {ConnectionId} - ErrorId {ErrorId}", connectionId, errorResponse.ServerError.ErrorId);
                await SendMessageAsync(webSocket, errorResponse);
            }
        }

        if (playerInfo != null)
        {
            _activeSessions.TryRemove(playerInfo.PlayerId, out _);
        }

        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session closed", CancellationToken.None);    
        }
        
    }

    private async Task<PlayerInfo?> ProcessLoginRequestAsync(WebSocket webSocket, MessageType messageType, string connectionId, MemoryStream inputStream)
    {
        if (messageType != MessageType.LoginRequest)
        {
            await SendErrorAsync(webSocket, "Unauthorized access attempt", connectionId);
            return null;
        }

        var playerInfo = new PlayerInfo();
        var serverResponse = await _handlers[MessageType.LoginRequest].HandleMessageAsync(playerInfo, inputStream);
        
        if (serverResponse is ServerResponse response && response.LoginResponse != null)
        {
            var playerId = response.LoginResponse.PlayerId;

            if (_activeSessions.ContainsKey(playerId))
            {
                await SendErrorAsync(webSocket, $"Player {playerId} already connected", connectionId);
                return null;
            }

            _activeSessions[playerId] = (webSocket, playerInfo);
        }

        await SendMessageAsync(webSocket, serverResponse);
        return playerInfo;
    }

    private async Task ProcessMessageAsync(PlayerInfo playerInfo, MessageType messageType, MemoryStream inputStream, string connectionId, WebSocket webSocket)
    {
        if (_handlers.TryGetValue(messageType, out var handler))
        {
            var response = await handler.HandleMessageAsync(playerInfo, inputStream);
            await SendMessageAsync(webSocket, response);
        }
        else
        {
            await SendErrorAsync(webSocket, "Invalid message type", connectionId);
        }
    }

    private async Task SendMessageAsync(WebSocket webSocket, IMessage message)
    {
        try
        {
            using var outputStream = new MemoryStream();
            message.WriteTo(outputStream);
            var bytes = outputStream.ToArray();

            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
    }

    private async Task SendErrorAsync(WebSocket webSocket, string errorMessage, string connectionId)
    {
        var errorResponse = CreateServerError(errorMessage);
        _logger.LogError("{ErrorMessage} - Connection {ConnectionId} - ErrorId {ErrorId}", errorMessage, connectionId, errorResponse.ServerError.ErrorId);
        await SendMessageAsync(webSocket, errorResponse);
    }

    private static ServerResponse CreateServerError(string message)
    {
        return new ServerResponse { ServerError = new ServerError { ErrorId = Guid.NewGuid().ToString(), Message = message } };
    }

    public void Dispose()
    {
        _notificationManager.OnMessageRecieved -= HandleIncomingMessageAsync;
    }
}