namespace GameServer.Common;

public interface IGameRepository
{
    Task<int> GetOrAddPlayerAsync(Guid deviceId);
    Task<int> UpdateResourceAsync(int playerId,ResourceType resourceType, int resourceValue);
    Task<int> TransferResource(int fromPlayer, int toPlayer, ResourceType resourceType, int resourceValue);
}