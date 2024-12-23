using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace GameServer.Common;

public interface ISessionManager
{
    PlayerSession? GetSession(string sessionId);
    void AddSession(string sessionId, PlayerSession session);
    bool RemoveSession(string sessionId);
}

public record PlayerSession(string SessionId)
{
    public string SessionId { get; } = SessionId;
    public int PlayerId { get; set; } 
}

public class SessionManager : ISessionManager//todo: implemet as redis
{
    private readonly ILogger<SessionManager> _logger;
    private readonly ConcurrentDictionary<string,PlayerSession> _sessions = new ConcurrentDictionary<string,PlayerSession>();

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }
    public PlayerSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out PlayerSession? session);
        return session;
    }

    public void AddSession(string sessionId, PlayerSession session)
    {
        if(_sessions.TryGetValue(sessionId, out _))
        {
            throw new Exception($"player {session.PlayerId} is already connected");
        }
        _sessions[sessionId] = session;
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