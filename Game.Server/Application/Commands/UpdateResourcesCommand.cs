using Game.Contracts;
using Game.Server;
using Game.Server.Common;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Commands;

public class UpdateResourcesCommand:ICommandHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.UpdateRequest;
    public MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourcesCommand(IGameRepository gameRepository)
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