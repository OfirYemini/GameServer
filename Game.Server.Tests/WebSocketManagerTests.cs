// using System.Net.WebSockets;
// using FluentAssertions;
// using Game.Contracts;
// using Game.Server.DataAccess;
// using GameServer.Core.Interfaces;
// using GameServer.Infrastructure;
// using Google.Protobuf;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Http.Features;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using NSubstitute;
// using WebSocketManager = GameServer.Infrastructure.WebSocketManager;
//
// namespace Game.Server.Tests;
//
// public class WebSocketManagerTests
// {
//         private readonly WebSocket _webSocket;
//         private readonly ICommandHandler _loginHandler;
//         private readonly HttpContext _httpContext;
//         private readonly List<ICommandHandler> _commands;
//         private readonly INotificationManager _notificationManager;
//         private readonly ILogger<WebSocketManager> _logger;
//
//
//         public WebSocketManagerTests()
//         {
//             _loginHandler = Substitute.For<ICommandHandler>();
//             _loginHandler.MessageType.Returns(MessageType.LoginRequest);
//             //_loginHandler.HandleMessageAsync(default,default).ReturnsForAnyArgs(new LoginResponse());
//             _logger = Substitute.For<ILogger<WebSocketManager>>();
//             _commands = new List<ICommandHandler>()
//             {
//                 _loginHandler
//             };
//             _webSocket = Substitute.For<WebSocket>();
//             _notificationManager = Substitute.For<INotificationManager>();
//             _httpContext = Substitute.For<HttpContext>();
//             var connectionInfo = Substitute.For<ConnectionInfo>();
//             connectionInfo.Id.Returns(Guid.NewGuid().ToString());
//             _httpContext.Connection.Returns(connectionInfo);
//             _webSocket.State.Returns(WebSocketState.Open);
//         }
//
//         [Fact]
//         public async Task HandleWebSocketSessionAsync_CallsLogin()
//         {
//             //Arrange
//             var webSocketManager = new WebSocketManager(_commands, _notificationManager, _logger);
//             var loginRequest = new LoginRequest
//             {
//                 DeviceId = Guid.NewGuid().ToString(),
//             };
//             byte[] bytes = new byte[1024 * 4];
//             using var stream = new MemoryStream();
//             loginRequest.WriteTo(stream);
//             bytes = stream.ToArray();
//
//             _webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
//                 .Returns(async x =>
//                 {
//                     x[0] = new ArraySegment<byte>(bytes);
//                 });
//                 // .Returns(async callInfo =>
//                 // {
//                 //     await Task.Delay(2);
//                 //     var buffer = callInfo.Arg<ArraySegment<byte>>();
//                 //     bytes.CopyTo(buffer.Array, buffer.Offset);
//                 // });
//             //Act
//             await webSocketManager.HandleWebSocketSessionAsync(_httpContext, _webSocket);
//             //Assert
//             await _webSocket.Received().ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>());
//         }
//
//     }