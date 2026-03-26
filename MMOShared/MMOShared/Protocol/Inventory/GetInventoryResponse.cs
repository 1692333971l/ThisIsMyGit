using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class GetInventoryResponse
    {
        // 错误码
        public int ErrorCode;

        // 返回消息
        public string Message;

        // 角色ID
        public int CharacterId;

        // 背包物品列表
        public List<InventoryItemInfo> ItemList;
    }
}