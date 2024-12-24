using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using Game.Server.Handlers;
using Game.Server.Common;
using GameServer.Application.Commands;
using GameServer.Core.Interfaces;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.Data;
using PlayerInfo = GameServer.Core.Entities.PlayerInfo;

namespace Game.Server;

using PlayerInfo = PlayerInfo;

public class WebSocketManager:IDisposable
{
    private readonly ILogger<WebSocketManager> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly Dictionary<MessageType,ICommandHandler> _handlers;
    private readonly ConcurrentDictionary<int, (WebSocket webSocket, PlayerInfo playerInfo)> _sessions = new();

    public WebSocketManager(IEnumerable<ICommandHandler> handlers,INotificationManager notificationManager,ILogger<WebSocketManager> logger)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _handlers = handlers.ToDictionary(k=>k.MessageType, v => v);
        
        _notificationManager.OnMessageRecieved += OnMessageRecieved;
    }
    
    private async Task OnMessageRecieved(int targetId, IMessage message)
    {
        try
        {
            if (_sessions.TryGetValue(targetId, out var session) && session.webSocket.State == WebSocketState.Open)
            {
                await SendMessageAsync(session.webSocket, message);
                return;
            }
            _logger.LogInformation($"No active session for {targetId}, notification message discarded");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending notification message to player {playerId}",targetId);
        }
    }
    public async Task HandleWebSocketSessionAsync(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        string connectionId = context.Connection.Id;
        PlayerInfo? playerInfo = null;
        _logger.LogInformation("New websocket connection with id {connectionId} is initiated",connectionId);
        while (webSocket.State == WebSocketState.Open)
        {
            IMessage serverResponse;
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Client requested close. Closing WebSocket...");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                using var inputStream = new MemoryStream(buffer, 0, result.Count);
                var messageType = (MessageType)inputStream.ReadByte();
                
                if (playerInfo == null || messageType == MessageType.LoginRequest)
                {
                    var firstMsgResult = await HandleFirstMessageAsync(webSocket,messageType, connectionId, inputStream); 
                    playerInfo ??= firstMsgResult;
                    continue;
                }
                
                if (_handlers.TryGetValue(messageType, out var handler))
                {
                    serverResponse = await handler.HandleMessageAsync(playerInfo, inputStream);
                }
                else
                {
                    serverResponse = CreateServerError("message type is invalid");
                    _logger.LogError(
                        "Invalid request with message type {messageType} was attempted for session {connectionId}, errorId: {error}",
                        messageType, connectionId,((ServerResponse)serverResponse).ServerError.ErrorId);
                }
            }
            catch (Exception e)
            {
                serverResponse = CreateServerError("An error has occured");
                _logger.LogError(e,"Websocket error for connection {connectionId} with errorId {error}",connectionId,((ServerResponse)serverResponse).ServerError.ErrorId);
            }
            await SendMessageAsync(webSocket, serverResponse);//todo: handle error here
        }
        
        if (playerInfo!=null)
        {
            _sessions.TryRemove(playerInfo.PlayerId, out _);    
        }
    }

    private async Task<PlayerInfo?> HandleFirstMessageAsync(WebSocket webSocket,MessageType messageType,string connectionId, MemoryStream inputStream)
    {
        IMessage serverResponse;
        PlayerInfo? playerInfo = null;
        
        if (messageType != MessageType.LoginRequest)
        {
            serverResponse = CreateServerError("player is not authenticated");
            _logger.LogError("Unauthenticated request was attempted with connection {connectionId}",connectionId);    
        }
        else
        {
            var newPlayerInfo = new PlayerInfo();
            serverResponse = await _handlers[MessageType.LoginRequest].HandleMessageAsync(newPlayerInfo, inputStream);
            var loginResponse = (serverResponse as ServerResponse)?.LoginResponse;
            if (loginResponse != null)
            {
                if (_sessions.TryGetValue(loginResponse.PlayerId, out _))
                {
                    serverResponse = CreateServerError($"player {loginResponse.PlayerId} is already connected");
                    _logger.LogError("player {loginResponse.PlayerId} is already connected, errorId {}", loginResponse.PlayerId, ((ServerResponse)serverResponse).ServerError.ErrorId);
                }
                else
                {
                    _sessions[loginResponse.PlayerId] = (webSocket,newPlayerInfo);
                    playerInfo = newPlayerInfo;    
                }
            }
        }
        await SendMessageAsync(webSocket, serverResponse);
        
        return playerInfo;
    }

    private static async Task SendMessageAsync(WebSocket webSocket, IMessage serverResponse)
    {
        using var outputStream = new MemoryStream();
        serverResponse.WriteTo(outputStream);
            
        byte[] bytes = outputStream.ToArray();

        await webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None
        );
    }

    private static ServerResponse CreateServerError(string message)
    {
        return new ServerResponse()
        {
            ServerError = new ServerError()
            {
                ErrorId = Guid.NewGuid().ToString(),
                Message = message,
            },
        };
    }

    public void Dispose()
    {
        _notificationManager.OnMessageRecieved -= OnMessageRecieved;
    }
}
