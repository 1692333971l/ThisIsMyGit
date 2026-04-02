namespace MMOServer.Config
{
    public class ShopItemConfig
    {
        public int ShopId { get; set; }
        public int ItemId { get; set; }
        public int Price { get; set; }
        public int IsLimited { get; set; }
        public int LimitCount { get; set; }
        public int Sort { get; set; }
    }
}