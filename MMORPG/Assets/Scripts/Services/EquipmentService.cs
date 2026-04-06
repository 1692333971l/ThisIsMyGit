using Protocol;
using System;
using UnityEngine;

//装备业务层
public class EquipmentService
{
    public event Action<GetEquipmentResponse> OnGetEquipmentResponse;//获取装备响应事件
    public event Action<EquipItemResponse> OnEquipItemResponse;//装备道具响应事件
    public event Action<UnequipItemResponse> OnUnequipItemResponse;//卸下装备响应事件

    //获取当前装备请求
    public void SendGetEquipmentRequest()
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        GetEquipmentRequest request = new GetEquipmentRequest()
        {
            CharacterId = characterInfo.CharacterId
        };

        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.GetEquipmentRequest,
            BodyJson = JsonUtility.ToJson(request)
        };

        GameApp.Instance.NetClient.SendMessage(message);
    }

    //获取当前装备响应
    public void HandleGetEquipmentResponse(NetMessage message)
    {
        GetEquipmentResponse response = JsonUtility.FromJson<GetEquipmentResponse>(message.BodyJson);
        if (response != null)
        {
            GameApp.Instance.PlayerEquipmentManager.SetEquipmentList(response.EquipmentList);
            GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        }

        OnGetEquipmentResponse?.Invoke(response);
    }

    //发送装备背包物品请求
    public void SendEquipItemRequest(int slotIndex)
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        EquipItemRequest request = new EquipItemRequest()
        {
            CharacterId = characterInfo.CharacterId,
            SlotIndex = slotIndex
        };

        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.EquipItemRequest,
            BodyJson = JsonUtility.ToJson(request)
        };

        GameApp.Instance.NetClient.SendMessage(message);
    }

    //装备背包物品响应
    public void HandleEquipItemResponse(NetMessage message)
    {
        EquipItemResponse response = JsonUtility.FromJson<EquipItemResponse>(message.BodyJson);

        if (response != null)
        {
            GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
            GameApp.Instance.PlayerEquipmentManager.SetEquipmentList(response.EquipmentList);
            GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        }

        OnEquipItemResponse?.Invoke(response);
    }

    //发送卸下装备请求
    public void SendUnequipItemRequest(int equipSlotType)
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        UnequipItemRequest request = new UnequipItemRequest()
        {
            CharacterId = characterInfo.CharacterId,
            EquipSlotType = equipSlotType
        };

        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.UnequipItemRequest,
            BodyJson = JsonUtility.ToJson(request)
        };

        GameApp.Instance.NetClient.SendMessage(message);
    }

    //卸下装备响应
    public void HandleUnequipItemResponse(NetMessage message)
    {
        UnequipItemResponse response = JsonUtility.FromJson<UnequipItemResponse>(message.BodyJson);

        if (response != null)
        {
            GameApp.Instance.PlayerInventoryManager.SetPlayerInventoryDict(response.ItemList);
            GameApp.Instance.PlayerEquipmentManager.SetEquipmentList(response.EquipmentList);
            GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);
        }

        OnUnequipItemResponse?.Invoke(response);
    }
}