using FluentAssertions;
using GameServer.Application.Handlers;
using GameServer.Core;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Game.Server.Tests;

public class HandlerTests
{
    private readonly IGameRepository _repository;
    private readonly ILogger<LoginHandler> _loginLogger;
    private readonly ILogger<SendGiftHandler> _sendGiftLogger;
    private readonly INotificationManager _notificationManager;

    public HandlerTests()
    {
        _repository = Substitute.For<IGameRepository>();
        _loginLogger = Substitute.For<ILogger<LoginHandler>>();
        _sendGiftLogger = Substitute.For<ILogger<SendGiftHandler>>();
        _notificationManager = Substitute.For<INotificationManager>();
    }

    [Theory]
    [InlineData("invalid-device-id")]
    public async Task LoginHandler_Should_Return_Error_For_Invalid_DeviceId(string deviceId)
    {
        // Arrange
        var handler = new LoginHandler(_repository, _loginLogger);
        var playerInfo = new PlayerInfo();
        var request = new LoginRequest { DeviceId = deviceId };
        using var stream = new MemoryStream(request.ToByteArray());

        // Act
        var response = await handler.HandleMessageAsync(playerInfo, stream);

        // Assert
        response.Should().BeOfType<ServerResponse>();
        var serverResponse = response as ServerResponse;
        serverResponse.Should().NotBeNull();
        serverResponse.ServerError.Should().NotBeNull();
        serverResponse.ServerError.Message.Should().Be(ErrorMessages.InvalidDeviceId);
    }

    [Fact]
    public async Task LoginHandler_Should_Process_Valid_LoginRequest()
    {
        // Arrange
        _repository.GetOrAddPlayerAsync(Arg.Any<Guid>()).Returns(123);
        var handler = new LoginHandler(_repository, _loginLogger);
        var playerInfo = new PlayerInfo();
        var request = new LoginRequest { DeviceId = Guid.NewGuid().ToString() };
        using var stream = new MemoryStream(request.ToByteArray());

        // Act
        var response = await handler.HandleMessageAsync(playerInfo, stream);

        // Assert
        response.Should().BeOfType<ServerResponse>();
        var serverResponse = response as ServerResponse;
        serverResponse.Should().NotBeNull();
        serverResponse.LoginResponse.Should().NotBeNull();
        serverResponse.LoginResponse.PlayerId.Should().Be(123);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task SendGiftHandler_Should_Return_Error_For_Invalid_GiftValue(int value)
    {
        // Arrange
        var handler = new SendGiftHandler(_repository, _notificationManager, _sendGiftLogger);
        var playerInfo = new PlayerInfo { PlayerId = 1 };
        var request = new SendGiftRequest { FriendPlayerId = 2, ResourceType = ResourceType.Rolls, ResourceValue = value };
        using var stream = new MemoryStream(request.ToByteArray());

        // Act
        var response = await handler.HandleMessageAsync(playerInfo, stream);

        // Assert
        response.Should().BeOfType<ServerResponse>();
        var serverResponse = response as ServerResponse;
        serverResponse.Should().NotBeNull();
        serverResponse.ServerError.Should().NotBeNull();
        serverResponse.ServerError.Message.Should().Be(ErrorMessages.AmountMustBePositive);
    }

    [Fact]
    public async Task SendGiftHandler_Should_Process_Valid_GiftRequest()
    {
        // Arrange
        _repository.TransferResource(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<GameServer.Core.Entities.ResourceType>(), Arg.Any<int>())
            .Returns(100);
        var handler = new SendGiftHandler(_repository, _notificationManager, _sendGiftLogger);
        var playerInfo = new PlayerInfo { PlayerId = 1 };
        var request = new SendGiftRequest { FriendPlayerId = 2, ResourceType = ResourceType.Rolls, ResourceValue = 10 };
        using var stream = new MemoryStream(request.ToByteArray());

        // Act
        var response = await handler.HandleMessageAsync(playerInfo, stream);

        // Assert
        response.Should().BeOfType<ServerResponse>();
        var serverResponse = response as ServerResponse;
        serverResponse.Should().NotBeNull();
        serverResponse.SendGiftResponse.Should().NotBeNull();
        serverResponse.SendGiftResponse.NewBalance.Should().Be(100);
    }
}
