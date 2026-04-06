using Protocol;
using System.Collections.Generic;

//玩家当前装备管理器
public class PlayerEquipmentManager
{
    private readonly Dictionary<int, EquipmentItemInfo> _equipmentDict = new Dictionary<int, EquipmentItemInfo>();

    //设置当前全部装备数据
    public void SetEquipmentList(List<EquipmentItemInfo> equipmentList)
    {
        _equipmentDict.Clear();

        if (equipmentList == null)
        {
            return;
        }

        foreach (EquipmentItemInfo item in equipmentList)
        {
            _equipmentDict[item.EquipSlotType] = item;
        }
    }

    //获取全部装备数据
    public Dictionary<int, EquipmentItemInfo> GetEquipmentDict()
    {
        return _equipmentDict;
    }

    //根据槽位类型获取装备数据
    public EquipmentItemInfo GetEquipmentBySlotType(int equipSlotType)
    {
        _equipmentDict.TryGetValue(equipSlotType, out EquipmentItemInfo equipmentItemInfo);
        return equipmentItemInfo;
    }

    //判断某个槽位是否有装备
    public bool HasEquipmentOnSlot(int equipSlotType)
    {
        if (!_equipmentDict.TryGetValue(equipSlotType, out EquipmentItemInfo equipmentItemInfo))
        {
            return false;
        }

        return equipmentItemInfo != null && equipmentItemInfo.ItemId > 0;
    }

    //清空当前装备数据
    public void Clear()
    {
        _equipmentDict.Clear();
    }
}