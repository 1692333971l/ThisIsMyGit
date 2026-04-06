using System;

namespace Protocol
{
    [Serializable]
    public class UnequipItemRequest
    {
        public int CharacterId;
        public int EquipSlotType; // 要卸下的装备槽位
    }
}