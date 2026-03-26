using System;

namespace Protocol
{
    [Serializable]
    public class InventoryItemInfo
    {
        // 背包格子索引
        public int SlotIndex;

        // 道具配置表ID
        public int ItemId;

        // 当前数量
        public int Count;
    }
}