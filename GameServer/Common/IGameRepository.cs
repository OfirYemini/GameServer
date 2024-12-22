namespace GameServer.Common;

public interface IGameRepository
{
    Task<int> GetOrAddPlayerAsync(Guid deviceId);
}