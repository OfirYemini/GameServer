 using Game.Contracts;
//
 namespace GameClient;
//
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System.Net.WebSockets;
// using System.Text;
//
// public class WebSocketClientService : BackgroundService
// {
//     private readonly ILogger<WebSocketClientService> _logger;
//     private ClientWebSocket _client;
//     private readonly Uri _serverUri = new("ws://localhost:5000/ws");
//
//     public WebSocketClientService(ILogger<WebSocketClientService> logger)
//     {
//         _logger = logger;
//         _client = new ClientWebSocket();
//     }
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         await ConnectWebSocketAsync(stoppingToken);
//
//         // Start listening for user input in a separate task
//         _ = Task.Run(() => ListenForUserInputAsync(stoppingToken), stoppingToken);
//
//         // Listen for messages from the server
//         await ListenForMessagesAsync(stoppingToken);
//     }
//
//     private async Task ConnectWebSocketAsync(CancellationToken stoppingToken)
//     {
//         try
//         {
//             _logger.LogInformation("Connecting to WebSocket server...");
//             await _client.ConnectAsync(_serverUri, stoppingToken);
//             _logger.LogInformation("Connected to server.");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to connect to server.");
//         }
//     }
//
//     private async Task ListenForUserInputAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             _logger.LogInformation("Enter command (login/update/sendgift/exit): ");
//             string? input = Console.ReadLine()?.ToLower();
//
//             switch (input)
//             {
//                 case "login":
//                     await SendLoginRequestAsync();
//                     break;
//                 case "update":
//                     await SendUpdateRequestAsync();
//                     break;
//                 case "sendgift":
//                     await SendSendGiftRequestAsync();
//                     break;
//                 case "exit":
//                     _logger.LogInformation("Exiting...");
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     _logger.LogWarning("Invalid command. Try again.");
//                     break;
//             }
//         }
//     }
//
//     private async Task ListenForMessagesAsync(CancellationToken stoppingToken)
//     {
//         var buffer = new byte[1024 * 4];
//
//         while (_client.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
//         {
//             var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
//
//             if (result.MessageType == WebSocketMessageType.Close)
//             {
//                 _logger.LogWarning("Server closed the connection.");
//                 break;
//             }
//
//             var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
//             _logger.LogInformation($"Message received from server: {message}");
//         }
//     }
//
//     private async Task SendLoginRequestAsync()
//     {
//         var loginRequest = new LoginRequest
//         {
//             DeviceId = Guid.NewGuid().ToString()
//         };
//
//         _logger.LogInformation("Sending login request...");
//         await SendMessageAsync(loginRequest, MessageType.LoginRequest);
//     }
//
//     private async Task SendUpdateRequestAsync()
//     {
//         var updateRequest = new UpdateRequest
//         {
//             ResourceType = UpdateRequest.Types.ResourceType.Coins,
//             ResourceValue = 100
//         };
//
//         _logger.LogInformation("Sending update request...");
//         await SendMessageAsync(updateRequest, MessageType.UpdateRequest);
//     }
//
//     private async Task SendSendGiftRequestAsync()
//     {
//         var sendGiftRequest = new SendGiftRequest
//         {
//             FriendPlayerId = 200,
//             ResourceType = SendGiftRequest.Types.ResourceType.Rolls,
//             ResourceValue = 5
//         };
//
//         _logger.LogInformation("Sending gift request...");
//         await SendMessageAsync(sendGiftRequest, MessageType.SendGiftRequest);
//     }
//
//     private async Task SendMessageAsync<T>(T message, MessageType messageType) where T : Google.Protobuf.IMessage<T>
//     {
//         using var stream = new MemoryStream();
//
//         // Prefix the message with the message type byte
//         stream.WriteByte((byte)messageType);
//
//         // Serialize Protobuf message
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
//         _logger.LogInformation($"Sent {messageType} message to server.");
//     }
//
//     public override async Task StopAsync(CancellationToken cancellationToken)
//     {
//         _logger.LogInformation("Shutting down WebSocket client...");
//         await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client stopping", cancellationToken);
//         _client.Dispose();
//         await base.StopAsync(cancellationToken);
//     }
// }
