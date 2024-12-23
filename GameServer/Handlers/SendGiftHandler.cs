using Game.Contracts;
using GameServer.Common;
using Google.Protobuf;

namespace GameServer.Handlers;

public interface INotificationManager
{
    void SendMessage(IMessage message);
}

public class SendGiftHandler:IWebSocketHandler
{
    private readonly IGameRepository _gameRepository;
    public MessageType MessageType { get; } = MessageType.SendGift;
    
    public SendGiftHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    public async Task<IMessage> HandleMessageAsync(PlayerSession session, MemoryStream stream)
    {
        SendGiftRequest request = SendGiftRequest.Parser.ParseFrom(stream);
        int newBalance = await _gameRepository.TransferResource(session.PlayerId,request.FriendPlayerId, (Common.ResourceType)request.ResourceType, -request.ResourceValue);
        var response = new SendGiftResponse(){NewBalance = newBalance};
        var serverResponse = new ServerResponse()
        {
            SendGiftResponse = response
        };
        return serverResponse;
    }
}