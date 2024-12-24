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
        var validationErrorMessage = ValidateRequest(info, request);
        if (validationErrorMessage != null)
        {
            return validationErrorMessage;
        }
        
        int newBalance = await _gameRepository.TransferResource(info.PlayerId,request.FriendPlayerId, (Common.ResourceType)request.ResourceType, request.ResourceValue);
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

    private IMessage? ValidateRequest(PlayerInfo info, SendGiftRequest request)
    {
        if (request.FriendPlayerId == info.PlayerId)
        {
            return CreateErrorMessage("Can't send gift to yourself");
        }
        if (request.ResourceValue <= 0)
        {
            return CreateErrorMessage("Can't send gift with zero or negative value");
        }
        return null;
    }

    private static IMessage? CreateErrorMessage(string message)
    {
        return new ServerResponse()
        {
            ServerError = new ServerError()
            {
                ErrorId = Guid.NewGuid().ToString(),
                Message = message
            }
        };
    }
}