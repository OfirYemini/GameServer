using System.Net.WebSockets;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

using System.Text;
using System.Text.Json;

public abstract class BaseWebSocketHandler<TRequest, TResponse> : IWebSocketHandler
{
    private readonly IWebSocketMessageSerializer _serializer;

    protected BaseWebSocketHandler(IWebSocketMessageSerializer serializer)
    {
        _serializer = serializer;
    }
    public abstract Request.InnerMessageOneofCase MessageDescriptor { get; }
    
    protected abstract MessageParser Parser { get; }
    public abstract Task<TResponse> HandleAsync(TRequest input);
    
    public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
    {
        try
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Deserialize into Request (wrapper for oneof messages)
            using var stream = new MemoryStream(buffer, 0, result.Count);
            // Deserialize the request
            var msg = Parser.ParseFrom();
        
            // Process the request
            var response = await HandleAsync(request);
        
            // Serialize and send the response
            await _serializer.SendMessageAsync(webSocket, response);
        
            // Close the WebSocket connection
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in handler for route '{Route}': {ex.Message}");
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "An error occurred", CancellationToken.None);
        }
    }

    

    
}
