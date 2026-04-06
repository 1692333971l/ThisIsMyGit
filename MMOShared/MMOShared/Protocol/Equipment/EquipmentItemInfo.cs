using System;

namespace Protocol
{
    [Serializable]
    public class EquipmentItemInfo
    {
        public int EquipSlotType; // 装备槽位类型
        public int ItemId;        // 当前槽位装备的道具ID，0表示空
    }
}