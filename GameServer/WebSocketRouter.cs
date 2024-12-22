using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.Data;

namespace GameServer;

public class WebSocketRouter
{
    private readonly Dictionary<Request.InnerMessageOneofCase, IWebSocketHandler> _handlers;
    
    public WebSocketRouter(IEnumerable<IWebSocketHandler> handlers)
    {
        _handlers = handlers.ToDictionary(k=>k.MessageDescriptor, v => v);
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

            // Read the first byte to determine the message type
            //Any any = Any.Parser.ParseFrom(stream);
            Request wrapper = Request.Parser.ParseFrom(stream);
            
            if (_handlers.TryGetValue(wrapper.InnerMessageCase, out var handler))
            {
                
                await handler.HandleWebSocketAsync(context, webSocket);
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
// public enum MessageType
// {
//     Login = 1,
//     Update = 2
// }