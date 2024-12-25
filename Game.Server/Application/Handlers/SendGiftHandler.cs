using Game.Contracts;
using GameServer.Core.Entities;
using GameServer.Core.Interfaces;
using Google.Protobuf;

namespace GameServer.Application.Handlers;

public class SendGiftHandler:BaseHandler<SendGiftRequest>
{
    private readonly IGameRepository _gameRepository;
    private readonly INotificationManager _notificationManager;
    public override MessageType MessageType { get; } = MessageType.SendGift;
    
    public SendGiftHandler(IGameRepository gameRepository,INotificationManager notificationManager,ILogger<SendGiftHandler> logger):base(SendGiftRequest.Parser,logger)
    {
        _gameRepository = gameRepository;
        _notificationManager = notificationManager;
    }
    
    protected override async Task<IMessage> ProcessAsync(PlayerInfo info, SendGiftRequest request)
    {
        int newBalance = await _gameRepository.TransferResource(info.PlayerId,request.FriendPlayerId, (Core.Entities.ResourceType)request.ResourceType, request.ResourceValue);
        var response = new SendGiftResponse(){NewBalance = newBalance};
        
        await NotifyGiftReciever(info, request);
        var serverResponse = new ServerResponse()
        {
            SendGiftResponse = response
        };
        return serverResponse;
    }

    private async Task NotifyGiftReciever(PlayerInfo info, SendGiftRequest request)
    {
        await _notificationManager.SendMessageAsync(request.FriendPlayerId,new ServerResponse(){GiftEvent = new GiftEvent()
        {
            FromPlayer = info.PlayerId,
            ResourceType = request.ResourceType,
            ResourceValue = request.ResourceValue,
        }});
    }

    protected override bool Validate(PlayerInfo info,SendGiftRequest request, out string? errorMessage)
    {
        errorMessage = null;
        if (request.FriendPlayerId == info.PlayerId)
        {
            errorMessage ="Can't send gift to yourself";
        }
        if (request.ResourceValue <= 0)
        {
            errorMessage ="Can't send gift with zero or negative value";
        }
        return errorMessage == null;
    }

    

    
}