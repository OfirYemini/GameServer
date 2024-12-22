using System.Net.WebSockets;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class UpdateResourcesHandler:BaseWebSocketHandler<UpdateResourceRequest, UpdateResourceResponse>
{
    public override Request.InnerMessageOneofCase MessageDescriptor { get; } = Request.InnerMessageOneofCase.UpdateRequest;
    protected override MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourcesHandler(IWebSocketMessageSerializer serializer) 
    : base(serializer)
    {
    }
    
    public override Task<UpdateResourceResponse> HandleAsync(UpdateResourceRequest input)
    {
        throw new NotImplementedException();
    }
}