using Game.Contracts;
using GameServer.Application.Interfaces;
using GameServer.Core.Entities;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public abstract class BaseHandler<TRequest> : IHandler where TRequest : IMessage<TRequest>
{
    private readonly MessageParser<TRequest> _parser;
    private readonly ILogger<IHandler> _logger;
    public abstract MessageType MessageType { get; }

    protected BaseHandler(MessageParser<TRequest> parser,ILogger<IHandler> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    public async Task<IMessage> HandleMessageAsync(PlayerInfo playerInfo, MemoryStream stream)
    {
        try
        {
            TRequest request = _parser.ParseFrom(stream);
            
            if (!Validate(playerInfo,request,out var errorMessage))
            {
                var errMsg = CreateErrorResponse(errorMessage!);
                _logger.LogError("Failed to process request,{errorMessage} error id {errorId}",errMsg.ServerError.Message, errMsg.ServerError.ErrorId);
                return errMsg;
            }
            
            return await ProcessAsync(playerInfo, request);
        }
        catch (Exception ex)
        {
            var errorResponse = CreateErrorResponse($"Failed to process request");
            _logger.LogError(ex,"Failed to process request, error id {errorId}",errorResponse.ServerError.ErrorId);
            return errorResponse;
        }
    }

    protected abstract bool Validate(PlayerInfo info,TRequest request, out string? errorMessage);

    protected abstract Task<IMessage> ProcessAsync(PlayerInfo info, TRequest request);

    private ServerResponse CreateErrorResponse(string message)
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