namespace GameServer.Common;

public interface IGameRepository
{
    Task<int> GetOrAddPlayerAsync(Guid deviceId);
    Task<int> UpdateResourceAsync(int playerId,ResourceType resourceType, int resourceValue);
}