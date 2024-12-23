﻿using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GameServer.Handlers;

public class LoginHandler:IWebSocketHandler
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.LoginRequest;
    
    public LoginHandler(ISessionManager sessionManager,IGameRepository gameRepository)
        
    {
        _sessionManager = sessionManager;
        _gameRepository = gameRepository;
    }

    public async Task<IMessage> HandleMessageAsync(PlayerSession session, MemoryStream stream)
    {
        LoginRequest request = LoginRequest.Parser.ParseFrom(stream);
        Guid deviceId = Guid.Parse(request.DeviceId);
        
        int playerId = await _gameRepository.GetOrAddPlayerAsync(deviceId);
        session.PlayerId = playerId;
        _sessionManager.AddSession(session.SessionId,session);
        
        var response = new ServerResponse()
        {
            LoginResponse = new LoginResponse(){PlayerId = playerId }
        };
        return response;
    }
}