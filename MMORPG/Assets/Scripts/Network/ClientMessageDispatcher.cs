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
            case MessageId.LoginResponse:
                GameApp.Instance.UserService.HandleLoginResponse(message);
                break;
            case MessageId.RegisterResponse:
                GameApp.Instance.UserService.HandleRegisterResponse(message);
                break;
            default:
                Debug.LogWarning($"ClientMessageDispatcher: unknown message id = {message.MessageId}");
                break;
        }
    }
}