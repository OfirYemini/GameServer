namespace GameServer.Infrastructure;

public class PlayerBalance
{
    public int PlayerId { get; set; }
    public byte ResourceType { get; set; }
    public int ResourceBalance { get; set; }
    
    public int RowVersion { get; set; }
}