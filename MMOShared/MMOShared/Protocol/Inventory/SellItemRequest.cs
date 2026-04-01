using System;

namespace Protocol
{
    [Serializable]
    public class SellItemRequest
    {
        // 数量
        public int Quantity;

        // 要使用的背包格子索引
        public int SlotIndex;
    }
}