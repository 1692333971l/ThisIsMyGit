using UnityEngine;
using Protocol;

//客户端消息分发层
//收到 NetMessage根据 MessageId转发给正确的业务模块
public class ClientMessageDispatcher
{
    /// <summary>
    /// 统一处理服务端返回的消息
    /// </summary>
    public void HandleMessage(NetMessage message)
    {
        switch ((MessageId)message.MessageId)
        {
            case MessageId.LoginResponse://登录响应
                GameApp.Instance.UserService.HandleLoginResponse(message);
                break;
            case MessageId.RegisterResponse://注册响应
                GameApp.Instance.UserService.HandleRegisterResponse(message);
                break;
            case MessageId.CreateCharacterResponse://创建角色响应
                GameApp.Instance.CharacterService.HandleCreateCharacterResponse(message);
                break;
            case MessageId.GetCharacterListResponse://获取角色一览响应
                GameApp.Instance.CharacterService.HandleGetCharacterListResponse(message);
                break;
            case MessageId.EnterGameResponse://进入游戏响应
                GameApp.Instance.WorldService.HandleEnterGameResponse(message);
                break;
            case MessageId.PlayerEnterNotify://角色进入通知
                GameApp.Instance.WorldService.HandlePlayerEnterNotify(message);
                break;
            case MessageId.PlayerLeaveNotify://角色离开通知
                GameApp.Instance.WorldService.HandlePlayerLeaveNotify(message);
                break;
            case MessageId.PlayerMoveNotify://角色移动通知
                GameApp.Instance.WorldService.HandlePlayerMoveNotify(message);
                break;
            case MessageId.GetInventoryResponse://获取背包响应
                GameApp.Instance.InventoryService.HandleGetInventoryResponse(message);
                break;
            case MessageId.UseItemResponse://获取背包响应
                GameApp.Instance.InventoryService.HandleUseItemResponse(message);
                break;
            default:
                Debug.LogWarning($"ClientMessageDispatcher: unknown message id = {message.MessageId}");
                break;
        }
    }
}