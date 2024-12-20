using System.Net.WebSockets;
using GameServer.Common;

namespace GameServer.Handlers;

public class UpdateResourcesHandler:BaseWebSocketHandler<UpdateResourceRequest, UpdateResourceResponse>
{
    public override string Route { get; } = "update";
    
    public UpdateResourcesHandler(WebSocketMessageSerializer serializer) 
    : base(serializer)
    {
    }
    
    public override Task<UpdateResourceResponse> HandleAsync(UpdateResourceRequest input)
    {
        throw new NotImplementedException();
    }
}