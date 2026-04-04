using Protocol;
using System;
using UnityEngine;

//世界消息Service
public class WorldService
{
    public event Action<EnterGameResponse> OnEnterGameResponse;//进入地图响应事件
    public event Action<PlayerEnterNotify> OnPlayerEnterNotify;//玩家加入广播事件
    public event Action<PlayerLeaveNotify> OnPlayerLeaveNotify;//玩家退出广播事件
    public event Action<PlayerMoveNotify> OnPlayerMoveNotify;//玩家移动广播事件
    public event Action<TeleportResponse> OnTeleportResponse;//地图传送响应事件

    //进入地图请求
    public void SendEnterGame()
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        EnterGameRequest enterGameRequest = new EnterGameRequest()
        {
            UserId = characterInfo.UserId,
            CharacterId = characterInfo.CharacterId
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.EnterGameRequest,
            BodyJson = JsonUtility.ToJson(enterGameRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }

    //进入地图响应
    public void HandleEnterGameResponse(NetMessage message)
    {
        EnterGameResponse response = JsonUtility.FromJson<EnterGameResponse>(message.BodyJson);
        OnEnterGameResponse?.Invoke(response);
    }

    //发送地图传送请求
    public void SendTeleportRequest(int targetPortalId)
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        TeleportRequest request = new TeleportRequest()
        {
            CharacterId = characterInfo.CharacterId,
            TargetPortalId = targetPortalId
        };

        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.TeleportRequest,
            BodyJson = JsonUtility.ToJson(request)
        };

        GameApp.Instance.NetClient.SendMessage(message);
    }

    //地图传送响应
    public void HandleTeleportResponse(NetMessage message)
    {
        TeleportResponse response = JsonUtility.FromJson<TeleportResponse>(message.BodyJson);
        OnTeleportResponse?.Invoke(response);
    }

    //玩家进入响应
    public void HandlePlayerEnterNotify(NetMessage message)
    {
        PlayerEnterNotify notify = JsonUtility.FromJson<PlayerEnterNotify>(message.BodyJson);
        OnPlayerEnterNotify?.Invoke(notify);
    }

    //玩家退出响应
    public void HandlePlayerLeaveNotify(NetMessage message)
    {
        PlayerLeaveNotify notify = JsonUtility.FromJson<PlayerLeaveNotify>(message.BodyJson);
        OnPlayerLeaveNotify?.Invoke(notify);
    }

    //玩家移动响应
    public void HandlePlayerMoveNotify(NetMessage message)
    {
        PlayerMoveNotify notify = JsonUtility.FromJson<PlayerMoveNotify>(message.BodyJson);
        OnPlayerMoveNotify?.Invoke(notify);
    }
}