namespace GameServer.Application.Interfaces;

public interface IGameRepository
{
    Task<int> GetOrAddPlayerAsync(Guid deviceId);
    Task<int> UpdateResourceAsync(int playerId,Core.Entities.ResourceType resourceType, int resourceValue);
    Task<int> TransferResource(int fromPlayer, int toPlayer, Core.Entities.ResourceType resourceType, int resourceValue);
}