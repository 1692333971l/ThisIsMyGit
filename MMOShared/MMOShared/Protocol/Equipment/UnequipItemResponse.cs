using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class UnequipItemResponse
    {
        public int ErrorCode;
        public string Message;

        public int CharacterId;

        // 最新背包
        public List<InventoryItemInfo> ItemList;

        // 最新装备栏
        public List<EquipmentItemInfo> EquipmentList;

        // 最新角色属性
        public CharacterInfo CharacterInfo;
    }
}