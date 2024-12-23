using System.Net.WebSockets;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class UpdateResourcesHandler:IWebSocketHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.UpdateRequest;
    public MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourcesHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    
    public async Task<IMessage> HandleMessageAsync(string sessionId,MemoryStream stream)
    {
        UpdateRequest request = UpdateRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.UpdateResourceAsync((Common.ResourceType)request.ResourceType, request.ResourceValue);
        var response = new UpdateResponse(){NewBalance = newBalance,Success = true};
        return response;
    }
}