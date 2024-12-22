using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class LoginHandler:BaseWebSocketHandler<DeviceLoginRequest, LoginResponse>
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameRepository _gameRepository;
    public override Request.InnerMessageOneofCase MessageDescriptor { get; } = Request.InnerMessageOneofCase.LoginRequest;
    protected override MessageParser Parser { get; } = LoginRequest.Parser;
    public override Task<LoginResponse> HandleAsync(DeviceLoginRequest input)
    {
        throw new NotImplementedException();
    }

    public LoginHandler(ISessionManager sessionManager,IGameRepository gameRepository, IWebSocketMessageSerializer serializer) 
        : base(serializer)
    {
        _sessionManager = sessionManager;
        _gameRepository = gameRepository;
    }

    public override async Task<LoginResponse> HandleAsync(IMessage<LoginRequest> request)
    {
        bool isNewConnection = await _sessionManager.TryAddAsync(request.DeviceId);
        if(!isNewConnection) throw new Exception($"device {request.DeviceId} is already connected");
        
        int playerId = await _gameRepository.GetOrAddPlayerAsync(request.DeviceId);
        return new LoginResponse(playerId);
    }
}