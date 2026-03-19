using MMOServer.Core;
using MMOServer.Services;
using Protocol;

namespace MMOServer.Network
{
    public class MessageDispatcher
    {
        private readonly UserService _userService;

        public MessageDispatcher()
        {
            _userService = new UserService();
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

                default:
                    Logger.Warn($"Unknown message id: {requestMessage.MessageId}");
                    return null;
            }
        }
    }
}