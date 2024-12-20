using System.Net.WebSockets;

namespace GameServer.Handlers;

using System.Text;
using System.Text.Json;

public abstract class BaseWebSocketHandler<TRequest, TResponse> : IWebSocketHandler
{
    private readonly WebSocketMessageSerializer _serializer;

    protected BaseWebSocketHandler(WebSocketMessageSerializer serializer)
    {
        _serializer = serializer;
    }
    public abstract string Route { get; }
    public abstract Task<TResponse> HandleAsync(TRequest input);
    
    public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
    {
        try
        {
            // Deserialize the request
            var request = await _serializer.ReceiveMessageAsync<TRequest>(webSocket);

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
