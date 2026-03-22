using Protocol;
using System;
using UnityEngine;

//角色选择创建Service
public class CharacterService
{
    public event Action<GetCharacterListResponse> OnGetCharacterListResponse;//获取角色一览响应事件
    public event Action<CreateCharacterResponse> OnCreateCharacterResponse;//创建角色响应事件
    public event Action<EnterGameResponse> OnEnterGameResponse;//进入主城响应事件

    //角色列表获取请求
    public void SendGetCharacterList()
    {
        GetCharacterListRequest characterListRequest = new GetCharacterListRequest()
        {
            UserId = GameApp.Instance.UserSession.GetUserId(),
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.GetCharacterListRequest,
            BodyJson = JsonUtility.ToJson(characterListRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //角色列表获取响应
    public void HandleGetCharacterListResponse(NetMessage message)
    {
        GetCharacterListResponse response = JsonUtility.FromJson<GetCharacterListResponse>(message.BodyJson);
        OnGetCharacterListResponse?.Invoke(response);
    }
    //创建角色请求
    public void SendCreateCharacter(string name, int profession)
    {
        CreateCharacterRequest createCharacterRequest = new CreateCharacterRequest()
        {
            UserId = GameApp.Instance.UserSession.GetUserId(),
            Name = name,
            Profession = profession
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.CreateCharacterRequest,
            BodyJson = JsonUtility.ToJson(createCharacterRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //创建角色响应
    public void HandleCreateCharacterResponse(NetMessage message)
    {
        CreateCharacterResponse response = JsonUtility.FromJson<CreateCharacterResponse>(message.BodyJson);
        OnCreateCharacterResponse?.Invoke(response);
    }
    //进入主城请求
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
            BodyJson= JsonUtility.ToJson(enterGameRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //进入主城响应
    public void HandleEnterGameResponse(NetMessage message)
    {
        EnterGameResponse response = JsonUtility.FromJson<EnterGameResponse>(message.BodyJson);
        OnEnterGameResponse?.Invoke(response);
    }
}
