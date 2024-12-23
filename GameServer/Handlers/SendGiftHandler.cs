using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;

namespace GameServer.Handlers;

public interface INotificationManager
{
    event Func<int, IMessage, Task> OnMessageRecieved;
    Task SendMessageAsync(int targetPlayerId, IMessage message);
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
    

    public async Task<IMessage> HandleMessageAsync(PlayerInfo info, MemoryStream stream)
    {
        SendGiftRequest request = SendGiftRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.TransferResource(info.PlayerId,request.FriendPlayerId, (Common.ResourceType)request.ResourceType, -request.ResourceValue);
        var response = new SendGiftResponse(){NewBalance = newBalance};
        
        await _notificationManager.SendMessageAsync(request.FriendPlayerId,new GiftEvent()
        {
            FromPlayer = info.PlayerId,
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