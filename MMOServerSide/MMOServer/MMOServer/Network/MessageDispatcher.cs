using MMOServer.Core;
using Protocol;

namespace MMOServer.Network
{
    public class MessageDispatcher
    {
        /// <summary>
        /// 根据消息号分发处理
        /// </summary>
        public NetMessage HandleMessage(NetMessage requestMessage, ClientSession session)
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

                case MessageId.EnterGameRequest:
                    return GameServer.Instance.WorldService.HandleEnterGame(requestMessage, session);

                case MessageId.PlayerMoveRequest:
                    GameServer.Instance.WorldService.HandlePlayerMove(requestMessage, session);
                    return null;

                case MessageId.PlayerExitRequest:
                    GameServer.Instance.WorldService.HandlePlayerExit(requestMessage, session);
                    return null;

                case MessageId.GetInventoryRequest:
                    return GameServer.Instance.InventoryService.HandleGetInventory(requestMessage);

                case MessageId.UseItemRequest:
                    return GameServer.Instance.InventoryService.HandleUseItem(requestMessage);
                default:
                    Logger.Warn($"Unknown message id: {requestMessage.MessageId}");
                    return null;
            }
        }
    }
}