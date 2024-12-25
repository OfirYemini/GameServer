using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public class LoginHandler:BaseHandler<LoginRequest>
{
    private readonly IGameRepository _gameRepository;
    public override MessageType MessageType { get; } = MessageType.LoginRequest;
    

    public LoginHandler(IGameRepository gameRepository,ILogger<LoginHandler> logger):base(LoginRequest.Parser,logger)
    {
        _gameRepository = gameRepository;
    }

    protected override bool Validate(PlayerInfo info, LoginRequest request, out string? errorMessage)
    {
        errorMessage = null;
        if(!Guid.TryParse(request.DeviceId,out _))
        {
            errorMessage = "Invalid Device ID";
        }
        return errorMessage == null;
    }

    protected override async Task<IMessage> ProcessAsync(PlayerInfo info, LoginRequest request)
    {
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