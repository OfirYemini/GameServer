using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Starting WebSocket client...");

using var client = new ClientWebSocket();

try
{
    // Connect to the WebSocket server
    var serverUri = new Uri("ws://localhost:5214/ws/chat"); // Adjust path to match server route
    await client.ConnectAsync(serverUri, CancellationToken.None);
    Console.WriteLine($"Connected to {serverUri}");

    // Send a message to the server
    string messageToSend = "Hello, WebSocket server!";
    var messageBytes = Encoding.UTF8.GetBytes(messageToSend);
    await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    Console.WriteLine($"Sent: {messageToSend}");

    // Receive a response from the server
    var buffer = new byte[1024 * 4];
    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    string serverResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
    Console.WriteLine($"Received: {serverResponse}");

    // Close the WebSocket connection
    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
    Console.WriteLine("WebSocket connection closed.");
}
catch (Exception ex)
{
    Console.WriteLine($"WebSocket error: {ex.Message}");
}