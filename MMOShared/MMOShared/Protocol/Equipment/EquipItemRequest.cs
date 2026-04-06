using System;

namespace Protocol
{
    [Serializable]
    public class EquipItemRequest
    {
        public int CharacterId;
        public int SlotIndex; // 背包格子索引
    }
}