namespace ConfigExporter.Models
{
    public class ItemConfig
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public int MaxStackCount { get; set; }
        public int SellPrice { get; set; }
        public int Quality { get; set; }
        public string IconPath { get; set; }
        public string Description { get; set; }

        // 使用相关
        public int CanUse { get; set; }
        public int UseEffectType { get; set; }
        public int UseEffectValue { get; set; }

        // 装备相关
        public int CanEquip { get; set; }
        public int EquipSlotType { get; set; }

        // 装备加成属性
        public int AddStrength { get; set; }
        public int AddAgility { get; set; }
        public int AddIntelligence { get; set; }
        public int AddDefense { get; set; }
        public int AddMaxHp { get; set; }
        public int AddMaxMp { get; set; }
        public int AddCritRate { get; set; }
        public int AddCritDamage { get; set; }
    }
}