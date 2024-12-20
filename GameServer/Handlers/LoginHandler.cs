using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameServer.Common;

namespace GameServer.Handlers;

public class LoginHandler:BaseWebSocketHandler<DeviceLoginRequest, LoginResponse>
{
    public override string Route { get; } = "login";
    public LoginHandler(WebSocketMessageSerializer serializer) 
        : base(serializer)
    {
    }

    public override Task<LoginResponse> HandleAsync(DeviceLoginRequest request)
    {
        var playerId = $"Player-{request.DeviceId}";
        return Task.FromResult(new LoginResponse (playerId));
    }
}