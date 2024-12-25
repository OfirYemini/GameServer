namespace Game.Contracts;

public enum MessageType:byte
{
    LoginRequest = 1,
    LoginResponse = 2,//todo:remove
    UpdateRequest = 3,
    SendGift = 4,
}