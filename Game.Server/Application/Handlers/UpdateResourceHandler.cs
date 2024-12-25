using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public class UpdateResourceHandler : BaseHandler<UpdateRequest>
{
    private readonly IGameRepository _gameRepository;
    public override MessageType MessageType { get; } = MessageType.UpdateRequest;

    public UpdateResourceHandler(IGameRepository gameRepository)
        : base(UpdateRequest.Parser)
    {
        _gameRepository = gameRepository;
    }

    protected override bool Validate(PlayerInfo info, UpdateRequest request, out string? errorMessage)
    {
        errorMessage = null;
        if(request.ResourceValue <= 0)
        {
            errorMessage = "Amount has to be a positive number";
        }
        return errorMessage == null;
    }

    protected override async Task<IMessage> ProcessAsync(PlayerInfo info, UpdateRequest request)
    {
        int newBalance = await _gameRepository.UpdateResourceAsync(
            info.PlayerId,
            (Core.Entities.ResourceType)request.ResourceType,
            request.ResourceValue
        );

        return new ServerResponse
        {
            UpdateResponse = new UpdateResponse { NewBalance = newBalance }
        };
    }
}