using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public class LoginHandler:IHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.LoginRequest;
    
    public LoginHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IMessage> HandleMessageAsync(PlayerInfo info, MemoryStream stream)
    {
        LoginRequest request = LoginRequest.Parser.ParseFrom(stream);
        Guid deviceId = Guid.Parse(request.DeviceId);
        
        int playerId = await _gameRepository.GetOrAddPlayerAsync(deviceId);
        info.PlayerId = playerId;
        
        var response = new ServerResponse()
        {
            LoginResponse = new LoginResponse(){PlayerId = playerId }
        };
        return response;
    }
}