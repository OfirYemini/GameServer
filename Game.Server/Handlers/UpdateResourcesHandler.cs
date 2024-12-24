﻿using System.Net.WebSockets;
using Game.Contracts;
using Game.Server.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Game.Server.Handlers;

public class UpdateResourcesHandler:IWebSocketHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.UpdateRequest;
    public MessageParser Parser { get; } = UpdateRequest.Parser;
    public UpdateResourcesHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    
    public async Task<IMessage> HandleMessageAsync(PlayerInfo info,MemoryStream stream)
    {
        UpdateRequest request = UpdateRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.UpdateResourceAsync(info.PlayerId,(Common.ResourceType)request.ResourceType, request.ResourceValue);
        var response = new UpdateResponse(){NewBalance = newBalance};
        var serverResponse = new ServerResponse()
        {
            UpdateResponse = response
        };
        return serverResponse;
    }
}