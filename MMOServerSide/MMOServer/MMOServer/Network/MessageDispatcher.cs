using MMOServer.Core;
using MMOServer.Services;
using Protocol;

namespace MMOServer.Network
{
    public class MessageDispatcher
    {
        /// <summary>
        /// 根据消息号分发处理
        /// </summary>
        public NetMessage HandleMessage(NetMessage requestMessage)
        {
            switch ((MessageId)requestMessage.MessageId)
            {
                case MessageId.LoginRequest:
                    return GameServer.Instance.UserService.HandleLogin(requestMessage);

                case MessageId.RegisterRequest:
                    return GameServer.Instance.UserService.HandleRegister(requestMessage);

                case MessageId.GetCharacterListRequest:
                    return GameServer.Instance.CharacterService.HandleGetCharacterList(requestMessage);

                case MessageId.CreateCharacterRequest:
                    return GameServer.Instance.CharacterService.HandleCreateCharacter(requestMessage);

                default:
                    Logger.Warn($"Unknown message id: {requestMessage.MessageId}");
                    return null;
            }
        }
    }
}