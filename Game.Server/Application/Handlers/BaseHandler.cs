using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public abstract class BaseHandler<TRequest> : IHandler where TRequest : IMessage<TRequest>
{
    private readonly MessageParser<TRequest> _parser;
    public abstract MessageType MessageType { get; }

    protected BaseHandler(MessageParser<TRequest> parser)
    {
        _parser = parser;
    }

    public async Task<IMessage> HandleMessageAsync(PlayerInfo playerInfo, MemoryStream stream)
    {
        try
        {
            TRequest request = _parser.ParseFrom(stream);
            
            if (!Validate(playerInfo,request,out var errorMessage))
            {
                return CreateErrorResponse(errorMessage!);
            }
            
            return await ProcessAsync(playerInfo, request);
        }
        catch (Exception ex)
        {
            //todo: add logger
            return CreateErrorResponse($"Failed to process request: {ex.Message}");
        }
    }

    protected abstract bool Validate(PlayerInfo info,TRequest request, out string? errorMessage);

    protected abstract Task<IMessage> ProcessAsync(PlayerInfo info, TRequest request);

    private IMessage CreateErrorResponse(string message)
    {
        return new ServerResponse
        {
            ServerError = new ServerError
            {
                ErrorId = Guid.NewGuid().ToString(),
                Message = message
            }
        };
    }
}