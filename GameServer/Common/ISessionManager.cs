using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace GameServer.Common;

public interface ISessionManager
{
    PlayerSession? GetSession(string sessionId);
    void CreateSession(string sessionId, PlayerSession session);
    Task<bool> TryRemoveAsync(Guid deviceId);
}

public record PlayerSession(int PlayerId);

public class SessionManager : ISessionManager//todo: implemet as redis
{
    private readonly ConcurrentDictionary<string,PlayerSession> _sessions = new ConcurrentDictionary<string,PlayerSession>();
    public PlayerSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out PlayerSession? session);
        return session;
    }

    public void CreateSession(string sessionId, PlayerSession session)
    {
        if(_sessions.TryGetValue(sessionId, out _))
        {
            throw new Exception($"player {session.PlayerId} is already connected");
        }
        _sessions[sessionId] = session;
    }

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