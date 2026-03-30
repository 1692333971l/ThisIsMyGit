using Protocol;
using System.Collections.Generic;

//背包格子管理器
public class PlayerInventoryManager
{
    private Dictionary<int, InventoryItemInfo> _playerInventoryDict = new Dictionary<int, InventoryItemInfo>();
    //初始化背包数据
    public void SetPlayerInventoryDict(List<InventoryItemInfo> itemList)
    {
        _playerInventoryDict.Clear();

        if (itemList == null)
        {
            return;
        }

        foreach (var item in itemList)
        {
            _playerInventoryDict[item.SlotIndex] = item;
        }
    }
    //获取背包全部数据
    public Dictionary<int, InventoryItemInfo> GetPlayerInventoryDict()
    {
        return _playerInventoryDict; 
    }
    //根据格子查询数据
    public InventoryItemInfo GetPlayerInventoryBySlotIndex(int SlotIndex)
    {
        _playerInventoryDict.TryGetValue(SlotIndex, out InventoryItemInfo inventoryItemInfo);
        return inventoryItemInfo;
    }
}
