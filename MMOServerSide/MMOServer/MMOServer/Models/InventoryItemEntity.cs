namespace MMOServer.Models
{
    //背包数据模型
    public class InventoryItemEntity
    {
        public int Id { get; set; }

        // 所属角色ID
        public int CharacterId { get; set; }

        // 道具配置表ID
        public int ItemId { get; set; }

        // 背包格子索引
        public int SlotIndex { get; set; }

        // 当前数量
        public int Count { get; set; }

        // 是否绑定
        public bool IsBound { get; set; }
    }
}