using Protocol;
using System;
using UnityEngine;

//背包业务层
public class InventoryService
{
    //获取背包请求
    public void SendGetInventoryRequest()
    {
        GetInventoryRequest getInventoryRequest = new GetInventoryRequest() 
        { 
            CharacterId = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo().CharacterId
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.GetInventoryRequest,
            BodyJson = JsonUtility.ToJson(getInventoryRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //获取背包响应
    public void HandleGetInventoryResponse(NetMessage message)
    {
        GetInventoryResponse response = JsonUtility.FromJson<GetInventoryResponse>(message.BodyJson);
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
    }
}
