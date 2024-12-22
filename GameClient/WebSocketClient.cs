namespace GameClient;

using Google.Protobuf;
using System.Net.WebSockets;
using System.Text;

public class WebSocketClient
{
    private readonly ClientWebSocket _client = new();
    private readonly Uri _serverUri;

    public WebSocketClient(string serverUrl)
    {
        _serverUri = new Uri(serverUrl);
    }

    public async Task ConnectAsync()
    {
        await _client.ConnectAsync(_serverUri, CancellationToken.None);
        Console.WriteLine("Connected to WebSocket server.");
    }

    public async Task SendLoginRequestAsync(string deviceId)
    {
        // Create LoginRequest
        var loginRequest = new LoginRequest
        {
            DeviceId = deviceId
        };

        // Wrap LoginRequest in Request (oneof)
        var wrapper = new Request
        {
            LoginRequest = loginRequest
        };

        await SendWrappedRequestAsync(wrapper);
        Console.WriteLine("Sent Login Request");
    }

    public async Task SendUpdateRequestAsync(string resourceType, int resourceValue)
    {
        // Create UpdateRequest
        var updateRequest = new UpdateRequest
        {
            ResourceType = resourceType,
            ResourceValue = resourceValue
        };

        // Wrap UpdateRequest in Request (oneof)
        var wrapper = new Request
        {
            UpdateRequest = updateRequest
        };

        await SendWrappedRequestAsync(wrapper);
        Console.WriteLine("Sent Update Request");
    }

    private async Task SendWrappedRequestAsync(Request request)
    {
        byte[] bytes = request.ToByteArray();

        await _client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None
        );
    }
}
