using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;

namespace GameServer.Handlers;

public interface INotificationManager
{
    void SendMessage(int targetPlayerId,IMessage message);
}

public class SendGiftHandler:IWebSocketHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly INotificationManager _notificationManager;
    public MessageType MessageType { get; } = MessageType.SendGift;
    
    public SendGiftHandler(IGameRepository gameRepository,INotificationManager notificationManager)
    {
        _gameRepository = gameRepository;
        _notificationManager = notificationManager;
    }
    public async Task<IMessage> HandleMessageAsync(PlayerSession session, MemoryStream stream)
    {
        SendGiftRequest request = SendGiftRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.TransferResource(session.PlayerId,request.FriendPlayerId, (Common.ResourceType)request.ResourceType, -request.ResourceValue);
        var response = new SendGiftResponse(){NewBalance = newBalance};
        
        _notificationManager.SendMessage(request.FriendPlayerId,new GiftEvent()
        {
            FromPlayer = session.PlayerId,
            ResourceType = request.ResourceType,
            ResourceValue = request.ResourceValue,
        });
        var serverResponse = new ServerResponse()
        {
            SendGiftResponse = response
        };
        return serverResponse;
    }
}