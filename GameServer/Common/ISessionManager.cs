using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace GameServer.Common;

public interface ISessionManager
{
    PlayerInfo? GetSession(string sessionId);
    void AddSession(string sessionId, PlayerInfo info);
    bool RemoveSession(string sessionId);
}

public record PlayerInfo()
{
    public int PlayerId { get; set; } 
}
