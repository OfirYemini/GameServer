namespace GameClient.Infrastructure;

using Game.Contracts;
using GameClient.Domain;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
public class GameClientService : IGameServerWs
{
    private readonly IWebSocketBackgroundService _wsBackgroundService;
    private readonly ILogger<GameClientService> _logger;

    public GameClientService(
        IWebSocketBackgroundService wsBackgroundService,
        ILogger<GameClientService> logger)
    {
        _wsBackgroundService = wsBackgroundService;
        _logger = logger;
    }

    public async Task LoginAsync(string deviceId)
    {
        var message = new LoginRequest()
        {
            DeviceId = deviceId,
        };
        await _wsBackgroundService.SendMessageAsync(MessageType.LoginRequest, message);
    }

    public async Task UpdateResourceAsync(ResourceType resourceType, int resourceValue)
    {
        var message = new UpdateRequest()
        {
            ResourceType = resourceType,
            ResourceValue = resourceValue
        };
        await _wsBackgroundService.SendMessageAsync(MessageType.UpdateRequest, message);
    }

    public async Task SendGiftAsync(int toPlayerId,ResourceType resourceType, int resourceValue)
    {
        var message = new SendGiftRequest()
        {
            FriendPlayerId = toPlayerId,
            ResourceType = resourceType,
            ResourceValue = resourceValue
        };
        await _wsBackgroundService.SendMessageAsync(MessageType.SendGift, message);
    }

    
}
