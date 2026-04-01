using Protocol;
using System;
using UnityEngine;

//背包业务层
public class InventoryService
{
    public event Action<GetInventoryResponse> OnGetInventoryResponse;//获取背包响应事件
    public event Action<UseItemResponse> OnUseItemResponse;//使用物品响应事件
    public event Action<SellItemResponse> OnSellItemResponse;//出售物品响应事件
    //获取背包请求
    public void SendGetInventoryRequest()
    {
        GetInventoryRequest getInventoryRequest = new GetInventoryRequest();
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
        OnGetInventoryResponse?.Invoke(response);
    }
    //使用物品请求
    public void SendUseItemRequest(int slotIndex)
    {
        UseItemRequest useItemRequest = new UseItemRequest()
        {
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
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        OnUseItemResponse?.Invoke(response);
    }
    //出售物品请求
    public void SendSellItemRequest(int quantity, int slotIndex)
    {
        SellItemRequest sellItemRequest = new SellItemRequest()
        {
            SlotIndex = slotIndex,
            Quantity = quantity
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.SellItemRequest,
            BodyJson = JsonUtility.ToJson(sellItemRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //出售物品响应
     public void HandleSellItemResponse(NetMessage message)
    {
        SellItemResponse response = JsonUtility.FromJson<SellItemResponse>(message.BodyJson);
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        OnSellItemResponse?.Invoke(response);
    }
}
