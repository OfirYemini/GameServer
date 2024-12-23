using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using GameServer.Common;
using GameServer.Handlers;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.Data;
using PlayerInfo = GameServer.Common.PlayerInfo;

namespace GameServer;

public class WebSocketManager:IDisposable
{
    private readonly ILogger<WebSocketManager> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly Dictionary<MessageType,IWebSocketHandler> _handlers;
    private readonly ConcurrentDictionary<int, (WebSocket webSocket, PlayerInfo playerInfo)> _sessions = new();

    public WebSocketManager(IEnumerable<IWebSocketHandler> handlers,INotificationManager notificationManager,ILogger<WebSocketManager> logger)
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
        string sessionId = context.Connection.Id;
        PlayerInfo? playerInfo = null;
        _logger.LogInformation("New websocket connection with id {connectionId} is initiated",sessionId);
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            using var inputStream = new MemoryStream(buffer, 0, result.Count);
            
            IMessage serverResponse;
            
            if (playerInfo == null)
            {
                playerInfo = await HandleFirstMessageAsync(webSocket, sessionId,inputStream);
                continue;
            }
            
            var messageType = (MessageType)inputStream.ReadByte();
            if (_handlers.TryGetValue(messageType, out var handler))
            {
                serverResponse = await handler.HandleMessageAsync(playerInfo,inputStream);
            }
            else
            {
                serverResponse = CreateServerError("message type is invalid");
                _logger.LogError("Invalid request with message type {messageType} was attempted for session {connectionId}",messageType,sessionId);
            }
            
            await SendMessageAsync(webSocket, serverResponse);
        }

        if (playerInfo!=null)
        {
            _sessions.TryRemove(playerInfo.PlayerId, out _);    
        }
        
        // else if (context.Request.Path == NoPath)
        // {
        //     Console.WriteLine("connection inititated");
        // }
        // else
        // {
        //     context.Response.StatusCode = StatusCodes.Status404NotFound;
        // }
    }

    private async Task<PlayerInfo?> HandleFirstMessageAsync(WebSocket webSocket, string sessionId, MemoryStream inputStream)
    {
        IMessage serverResponse;
        PlayerInfo? playerInfo = null;
        var messageType = (MessageType)inputStream.ReadByte();
        
        if (messageType != MessageType.LoginRequest)
        {
            serverResponse = CreateServerError("player is not authenticated");
            _logger.LogError("Unauthenticated request was attempted with connection {connectionId}",sessionId);    
        }
        else
        {
            var newPlayerInfo = new PlayerInfo(sessionId);
            serverResponse = await _handlers[MessageType.LoginRequest].HandleMessageAsync(newPlayerInfo, inputStream);
            var loginResponse = (serverResponse as ServerResponse)?.LoginResponse;
            if (loginResponse != null)
            {
                if(_sessions.TryGetValue(loginResponse.PlayerId, out _))
                {
                    throw new Exception($"player {loginResponse.PlayerId} is already connected");
                }
                _sessions[loginResponse.PlayerId] = (webSocket,newPlayerInfo);
                playerInfo = newPlayerInfo;
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

    private static IMessage CreateServerError(string message)
    {
        return new ServerResponse()
        {
            ServerError = new ServerError()
            {
                Message = message,
            },
        };
    }

    public void Dispose()
    {
        _notificationManager.OnMessageRecieved -= OnMessageRecieved;
    }
}
