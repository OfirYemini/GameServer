using System.Net.WebSockets;

namespace GameServer;

using System.Text;
using System.Text.Json;

public class WebSocketMessageSerializer
{
    private T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json) 
               ?? throw new InvalidOperationException("Invalid JSON received.");
    }

    private string Serialize<T>(T message)
    {
        return JsonSerializer.Serialize(message);
    }

    public async Task<T> ReceiveMessageAsync<T>(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var messageBytes = new List<byte>();
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);//todo: replace ct
            messageBytes.AddRange(buffer.Take(result.Count));
        } while (!result.EndOfMessage);

        var json = Encoding.UTF8.GetString(messageBytes.ToArray());
        return Deserialize<T>(json);
    }

    public async Task SendMessageAsync<T>(WebSocket webSocket, T message)
    {
        var json = Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
