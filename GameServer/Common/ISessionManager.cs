using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace GameServer.Common;

public interface ISessionManager
{
    Task<bool> TryAddAsync(Guid deviceId);
    Task<bool> TryRemoveAsync(Guid deviceId);
}

public class SessionManager : ISessionManager//todo: implemet as redis
{
    private readonly HashSet<Guid> _sessions = new HashSet<Guid>();
    public async Task<bool> TryAddAsync(Guid deviceId)
    {
        await Task.Delay(10);
        return _sessions.Add(deviceId);
    }

    public async Task<bool> TryRemoveAsync(Guid deviceId)
    {
        await Task.Delay(10);
        return _sessions.Remove(deviceId);
    }
}