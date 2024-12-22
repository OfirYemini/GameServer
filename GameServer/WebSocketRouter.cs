using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.Data;

namespace GameServer;

public class WebSocketRouter
{
    private readonly Dictionary<MessageType,IWebSocketHandler> _handlers;
    
    public WebSocketRouter(IEnumerable<IWebSocketHandler> handlers)
    {
        _handlers = handlers.ToDictionary(k=>k.MessageType, v => v);
    }
    
    public async Task RouteAsync(HttpContext context, WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            //var msg = await ReceiveMessageAsync(webSocket);
            //string msgType = ExtractMessageType(msg);
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            using var stream = new MemoryStream(buffer, 0, result.Count);

            // 1. Read the first byte (message type)
            // Read the first byte as enum
            var messageType = (MessageType)stream.ReadByte();

            
            if (_handlers.TryGetValue(messageType, out var handler))
            {
                var response = await handler.HandleMessageAsync(stream);
                
                using var responseStream = new MemoryStream();

                response.WriteTo(responseStream);

                byte[] bytes = responseStream.ToArray();

                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None
                );
            }
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
