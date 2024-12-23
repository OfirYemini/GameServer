namespace GameClient.Domain;

public interface IGameServerWs
{
    event Action<ServerResponse>? OnMessageReceived;
    Task LoginAsync(string deviceId);
    Task UpdateResourceAsync(ResourceType resourceType, int resourceValue);
    Task SendGiftAsync(int toPlayerId,ResourceType resourceType, int resourceValue);
}