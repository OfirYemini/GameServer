namespace GameServer.Common;

public enum ResourceType
{
    Coins = 0,
    Rolls = 1,
}

public record UpdateResourceRequest(ResourceType ResourceType);
public record UpdateResourceResponse(int PlayerBalance);