using MMOServer.Core;
using MMOServer.Services;
using Protocol;

namespace MMOServer.Network
{
    public class MessageDispatcher
    {
        private readonly UserService _userService;
        private readonly CharacterService _characterService;

        public MessageDispatcher()
        {
            _userService = new UserService();
            _characterService = new CharacterService();
        }

        /// <summary>
        /// 根据消息号分发处理
        /// </summary>
        public NetMessage HandleMessage(NetMessage requestMessage)
        {
            Logger.Info($"Dispatcher receive MessageId = {requestMessage.MessageId}, BodyJson = {requestMessage.BodyJson}");

            switch ((MessageId)requestMessage.MessageId)
            {
                case MessageId.LoginRequest:
                    return _userService.HandleLogin(requestMessage);
                case MessageId.RegisterRequest:
                    return _userService.HandleRegister(requestMessage);
                case MessageId.GetCharacterListRequest:
                    return _characterService.HandleGetCharacterList(requestMessage);
                case MessageId.CreateCharacterRequest:
                    return _characterService.HandleCreateCharacter(requestMessage);

                default:
                    Logger.Warn($"Unknown message id: {requestMessage.MessageId}");
                    return null;
            }
        }
    }
}