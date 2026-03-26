using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class AddItemResponse
    {
        // 错误码
        public int ErrorCode;

        // 返回消息
        public string Message;

        // 角色ID
        public int CharacterId;

        // 更新后的背包物品列表
        public List<InventoryItemInfo> ItemList;
    }
}