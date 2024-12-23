// using Google.Protobuf;
// using System.Net.WebSockets;
// using Game.Contracts;
//
// public class WebSocketClient
// {
//     private readonly ClientWebSocket _client = new();
//     private readonly Uri _serverUri;
//     private Type _expectedResponseType;
//
//     public WebSocketClient(string serverUrl)
//     {
//         _serverUri = new Uri(serverUrl);
//     }
//
//     public async Task ConnectAsync()
//     {
//         await _client.ConnectAsync(_serverUri, CancellationToken.None);
//         Console.WriteLine("Connected to WebSocket server.");
//     }
//
//     public async Task SendLoginRequestAsync(string deviceId)
//     {
//         var loginRequest = new LoginRequest
//         {
//             DeviceId = deviceId
//         };
//
//         _expectedResponseType = typeof(LoginResponse);
//         await SendProtobufMessageAsync(loginRequest, MessageType.LoginRequest);
//     }
//
//     public async Task SendUpdateRequestAsync(ResourceType resourceType, int value)
//     {
//         var updateRequest = new UpdateRequest
//         {
//             ResourceType = resourceType,
//             ResourceValue = value
//         };
//
//         _expectedResponseType = typeof(UpdateResponse);
//         await SendProtobufMessageAsync(updateRequest, MessageType.UpdateRequest);
//     }
//
//     private async Task SendProtobufMessageAsync<T>(T message, MessageType messageType) where T : IMessage<T>
//     {
//         using var stream = new MemoryStream();
//
//         // Write the message type as the first byte
//         stream.WriteByte((byte)messageType);
//
//         // Serialize the Protobuf message
//         message.WriteTo(stream);
//
//         byte[] bytes = stream.ToArray();
//
//         await _client.SendAsync(
//             new ArraySegment<byte>(bytes),
//             WebSocketMessageType.Binary,
//             true,
//             CancellationToken.None
//         );
//
//         Console.WriteLine($"Sent {typeof(T).Name} with messageType: {(byte)messageType}");
//     }
//
//     public async Task ListenForResponsesAsync()
//     {
//         var buffer = new byte[1024 * 4];
//         
//         var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//         using var stream = new MemoryStream(buffer, 0, result.Count);
//
//         ServerResponse serverResponse = ServerResponse.Parser.ParseFrom(stream);
//         
//         // Deserialize based on expected response type
//         if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.LoginResponse)
//         {
//             var response = serverResponse.LoginResponse;
//             Console.WriteLine($"LoginResponse: PlayerId={response.PlayerId}");
//         }
//         else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.UpdateResponse)
//         {
//             var response = serverResponse.UpdateResponse;
//             Console.WriteLine($"UpdateResponse: New Balance={response.NewBalance}");
//         }
//         else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.ServerError)
//         {
//             var response = serverResponse.ServerError;
//             Console.WriteLine($"Unexpected error received. {response.Message}");
//         }
//         else
//         {
//             Console.WriteLine("Unexpected response received.");
//         }
//     }
// }
