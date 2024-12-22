using System.Net.WebSockets;

namespace GameServer;

using System.Text;
using System.Text.Json;

public interface IWebSocketMessageSerializer
{
    T Deserialize<T>(string json);
    string Serialize<T>(T message);
    //Task<T> ReceiveMessageAsync<T>(WebSocket webSocket);
    //Task SendMessageAsync<T>(WebSocket webSocket, T message);
}

public class WebSocketMessageSerializer : IWebSocketMessageSerializer//todo: check SRP
{
    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json) 
               ?? throw new InvalidOperationException("Invalid JSON received.");
    }

    public string Serialize<T>(T message)
    {
        return JsonSerializer.Serialize(message);
    }

    public async Task<byte[]> ReceiveMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var messageBytes = new List<byte>();
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//todo: replace ct
            messageBytes.AddRange(buffer.Take(result.Count));
        } while (!result.EndOfMessage);

        return messageBytes.ToArray();
    }

    public async Task SendMessageAsync(WebSocket webSocket, byte[] payload) {
        
        await webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
