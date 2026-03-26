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
        public int CanUse { get; set; }
        public int UseEffectType { get; set; }
        public int UseEffectValue { get; set; }
    }
}