using System;
using System.Collections.Generic;

namespace Protocol
{
    [Serializable]
    public class BuyShopItemResponse
    {
        // 错误码
        public int ErrorCode;

        // 返回消息
        public string Message;

        // 当前角色ID
        public int CharacterId;

        // 商店ID
        public int ShopId;

        // 购买的道具ID
        public int ItemId;

        // 本次购买数量
        public int BuyQuantity;

        // 剩余限购数量（不限购可返回 -1）
        public int RemainingLimitCount;

        // 购买后的角色信息
        public CharacterInfo CharacterInfo;

        // 购买后的背包物品列表
        public List<InventoryItemInfo> ItemList;
    }
}