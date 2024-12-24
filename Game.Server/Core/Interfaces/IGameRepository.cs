namespace GameServer.Core.Interfaces;

public interface IGameRepository
{
    Task<int> GetOrAddPlayerAsync(Guid deviceId);
    Task<int> UpdateResourceAsync(int playerId,Entities.ResourceType resourceType, int resourceValue);
    Task<int> TransferResource(int fromPlayer, int toPlayer, Entities.ResourceType resourceType, int resourceValue);
}