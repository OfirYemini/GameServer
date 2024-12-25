using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public class UpdateResourceHandler:IHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.UpdateRequest;
    public MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourceHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    
    public async Task<IMessage> HandleMessageAsync(PlayerInfo info,MemoryStream stream)
    {
        UpdateRequest request = UpdateRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.UpdateResourceAsync(info.PlayerId,(Core.Entities.ResourceType)request.ResourceType, request.ResourceValue);
        var response = new UpdateResponse(){NewBalance = newBalance};
        var serverResponse = new ServerResponse()
        {
            UpdateResponse = response
        };
        return serverResponse;
    }
}