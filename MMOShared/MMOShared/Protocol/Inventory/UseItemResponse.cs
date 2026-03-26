using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class UseItemResponse
    {
        // 错误码
        public int ErrorCode;

        // 返回消息
        public string Message;

        // 角色ID
        public int CharacterId;

        // 使用后的背包物品列表
        public List<InventoryItemInfo> ItemList;
    }
}