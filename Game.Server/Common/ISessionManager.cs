using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace Game.Server.Common;

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
