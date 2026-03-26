using System;

namespace Protocol
{
    [Serializable]
    public class AddItemRequest
    {
        // 角色ID
        public int CharacterId;

        // 道具ID
        public int ItemId;

        // 增加数量
        public int Count;
    }
}