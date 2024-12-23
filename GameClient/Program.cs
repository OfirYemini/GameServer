

using GameClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//
// var host = Host.CreateDefaultBuilder(args)
//     .ConfigureServices(services =>
//     {
//         services.AddHostedService<WebSocketClientService>();
//     })
//     .Build();
//
// await host.RunAsync();

var client = new WebSocketClient("ws://localhost:5214/ws");

await client.ConnectAsync();


// Send a login request
await client.SendLoginRequestAsync("550e8400-e29b-41d4-a716-446655440000");
await client.ListenForResponsesAsync();
// Simulate a delay, then send an update request
await Task.Delay(2000);
await client.SendUpdateRequestAsync(ResourceType.Coins, 100);
await client.ListenForResponsesAsync();