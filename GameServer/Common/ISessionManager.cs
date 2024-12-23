using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace GameServer.Common;

public interface ISessionManager
{
    PlayerInfo? GetSession(string sessionId);
    void AddSession(string sessionId, PlayerInfo info);
    bool RemoveSession(string sessionId);
}

public record PlayerInfo(string SessionId)
{
    public string SessionId { get; } = SessionId;
    public int PlayerId { get; set; } 
}

public class SessionManager : ISessionManager//todo: implemet as redis
{
    private readonly ILogger<SessionManager> _logger;
    private readonly ConcurrentDictionary<string,PlayerInfo> _sessions = new ConcurrentDictionary<string,PlayerInfo>();

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }
    public PlayerInfo? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out PlayerInfo? session);
        return session;
    }

    public void AddSession(string sessionId, PlayerInfo info)
    {
        if(_sessions.TryGetValue(sessionId, out _))
        {
            throw new Exception($"player {info.PlayerId} is already connected");
        }
        _sessions[sessionId] = info;
    }

    public bool RemoveSession(string sessionId)
    {
        bool isSessionRemoved = _sessions.TryRemove(sessionId, out _);
        if (!isSessionRemoved)
        {
            _logger.LogError("session {sessionId} is not connected",sessionId);
        }

        return isSessionRemoved;
    }
}