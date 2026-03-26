using System;

namespace Protocol
{
    [Serializable]
    public class UseItemRequest
    {
        // 角色ID
        public int CharacterId;

        // 要使用的背包格子索引
        public int SlotIndex;
    }
}