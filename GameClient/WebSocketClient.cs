using Google.Protobuf;
using System.Net.WebSockets;
using Game.Contracts;

public class WebSocketClient
{
    private readonly ClientWebSocket _client = new();
    private readonly Uri _serverUri;
    private Type _expectedResponseType;

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
        var loginRequest = new LoginRequest
        {
            DeviceId = deviceId
        };

        _expectedResponseType = typeof(LoginResponse);
        await SendProtobufMessageAsync(loginRequest, MessageType.LoginRequest);
    }

    public async Task SendUpdateRequestAsync(string resourceType, int value)
    {
        var updateRequest = new UpdateRequest
        {
            ResourceType = resourceType,
            ResourceValue = value
        };

        _expectedResponseType = typeof(UpdateResponse);
        await SendProtobufMessageAsync(updateRequest, MessageType.UpdateRequest);
    }

    private async Task SendProtobufMessageAsync<T>(T message, MessageType messageType) where T : IMessage<T>
    {
        using var stream = new MemoryStream();

        // Write the message type as the first byte
        stream.WriteByte((byte)messageType);

        // Serialize the Protobuf message
        message.WriteTo(stream);

        byte[] bytes = stream.ToArray();

        await _client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None
        );

        Console.WriteLine($"Sent {typeof(T).Name} with messageType: {(byte)messageType}");
    }

    public async Task ListenForResponsesAsync()
    {
        var buffer = new byte[1024 * 4];
        
        var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        using var stream = new MemoryStream(buffer, 0, result.Count);

        // Deserialize based on expected response type
        if (_expectedResponseType == typeof(LoginResponse))
        {
            var response = LoginResponse.Parser.ParseFrom(stream);
            Console.WriteLine($"LoginResponse: PlayerId={response.PlayerId}");
        }
        else if (_expectedResponseType == typeof(UpdateResponse))
        {
            var response = UpdateResponse.Parser.ParseFrom(stream);
            Console.WriteLine($"UpdateResponse: New Balance={response.ResourceBalance}");
        }
        else
        {
            Console.WriteLine("Unexpected response received.");
        }
        
    }
}
