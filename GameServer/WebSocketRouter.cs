using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.Data;

namespace GameServer;

public class WebSocketRouter
{
    private readonly ISessionManager _sessionManager;
    private readonly Dictionary<MessageType,IWebSocketHandler> _handlers;
    
    public WebSocketRouter(ISessionManager sessionManager, IEnumerable<IWebSocketHandler> handlers)
    {
        _sessionManager = sessionManager;
        _handlers = handlers.ToDictionary(k=>k.MessageType, v => v);
    }
    
    public async Task RouteAsync(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        string sessionId = context.Connection.Id;
        while (webSocket.State == WebSocketState.Open)
        {
            //var msg = await ReceiveMessageAsync(webSocket);
            //string msgType = ExtractMessageType(msg);
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var playerSession = _sessionManager.GetSession(sessionId);
            
            using var inputstream = new MemoryStream(buffer, 0, result.Count);
            using var outputStream = new MemoryStream();
            
            // 1. Read the first byte (message type)
            // Read the first byte as enum
            var messageType = (MessageType)inputstream.ReadByte();
            if (playerSession == null && messageType != MessageType.LoginRequest)
            {
                ServerError error = new ServerError()
                {
                    Message = "player is not authenticated",
                };
                error.WriteTo(outputStream);
            }
            else if (messageType == MessageType.LoginRequest)
            {
                
            }
            else if (_handlers.TryGetValue(messageType, out var handler))
            {
                var response = await handler.HandleMessageAsync(inputstream);
                response.WriteTo(outputStream);
            }
            
            byte[] bytes = outputStream.ToArray();

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None
            );
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

    
    
    
    
}
