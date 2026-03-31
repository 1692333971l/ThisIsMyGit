using Protocol;
using System;
using UnityEngine;

//背包业务层
public class InventoryService
{
    public event Action OnGetInventoryResponse;//获取背包响应事件
    public event Action<UseItemResponse> OnUseItemResponse;//使用物品响应事件
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
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("获取背包失败，错误码：" + response.ErrorCode);
        }
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
        OnGetInventoryResponse?.Invoke();
    }
    //使用物品请求
    public void SendUseItemRequest(int slotIndex)
    {
        UseItemRequest useItemRequest = new UseItemRequest()
        {
            CharacterId = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo().CharacterId,
            SlotIndex = slotIndex
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.UseItemRequest,
            BodyJson = JsonUtility.ToJson(useItemRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //使用物品响应
    public void HandleUseItemResponse(NetMessage message)
    {
        UseItemResponse response = JsonUtility.FromJson<UseItemResponse>(message.BodyJson);
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("使用物品失败，错误码：" + response.ErrorCode);
        }
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        OnUseItemResponse?.Invoke(response);
        OnGetInventoryResponse?.Invoke();
    }
}
