using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class LoginHandler:IWebSocketHandler
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.LoginRequest;
    
    public LoginHandler(ISessionManager sessionManager,IGameRepository gameRepository)
        
    {
        _sessionManager = sessionManager;
        _gameRepository = gameRepository;
    }

    public async Task<IMessage> HandleMessageAsync(MemoryStream stream)
    {
        LoginRequest request = LoginRequest.Parser.ParseFrom(stream);
        Guid deviceId = Guid.Parse(request.DeviceId);
        bool isNewConnection = await _sessionManager.TryAddAsync(deviceId);
        if(!isNewConnection) throw new Exception($"device {request.DeviceId} is already connected");
        
        int playerId = await _gameRepository.GetOrAddPlayerAsync(deviceId);
        var response = new LoginResponse(){PlayerId = playerId};
        return response;
    }
}