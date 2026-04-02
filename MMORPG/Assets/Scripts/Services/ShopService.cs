//商店业务
using Protocol;
using System;
using UnityEngine;

public class ShopService
{
    public event Action<BuyShopItemResponse> OnBuyShopItemResponse;//购买商店物品响应事件

    //购买道具请求
    public void SendBuyShopItemRequest(int shopId, int itemId, int quantity)
    {
        BuyShopItemRequest buyShopItemRequest = new BuyShopItemRequest()
        {
            ShopId = shopId,
            ItemId = itemId,
            Quantity = quantity,
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.BuyShopItemRequest,
            BodyJson = JsonUtility.ToJson(buyShopItemRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //购买道具响应
    public void HandleBuyShopItemResponse(NetMessage message)
    {
        BuyShopItemResponse response = JsonUtility.FromJson<BuyShopItemResponse>(message.BodyJson);
        GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        OnBuyShopItemResponse?.Invoke(response);
    }
}
