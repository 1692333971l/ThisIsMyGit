using System;

namespace Protocol
{
    [Serializable]
    public class BuyShopItemRequest
    {
        // 商店ID
        public int ShopId;

        // 购买的道具ID
        public int ItemId;

        // 购买数量
        public int Quantity;
    }
}