using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using Game.Server.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Game.Server.Handlers;

public class LoginHandler:IWebSocketHandler
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