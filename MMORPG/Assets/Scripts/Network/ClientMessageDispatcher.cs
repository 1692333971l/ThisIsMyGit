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
            case MessageId.LoginResponse://登录请求
                GameApp.Instance.UserService.HandleLoginResponse(message);
                break;
            case MessageId.RegisterResponse://注册请求
                GameApp.Instance.UserService.HandleRegisterResponse(message);
                break;
            case MessageId.CreateCharacterResponse://创建角色请求
                GameApp.Instance.CharacterService.HandleCreateCharacterResponse(message);
                break;
            case MessageId.GetCharacterListResponse://获取角色一览请求
                GameApp.Instance.CharacterService.HandleGetCharacterListResponse(message);
                break;
            case MessageId.EnterGameResponse://进入游戏响应
                GameApp.Instance.CharacterService.HandleEnterGameResponse(message);
                break;
            default:
                Debug.LogWarning($"ClientMessageDispatcher: unknown message id = {message.MessageId}");
                break;
        }
    }
}