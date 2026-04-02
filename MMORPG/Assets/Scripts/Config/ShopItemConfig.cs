using System;

//商店配置表结构
[Serializable]
public class ShopItemConfig
{
    public int ShopId;
    public int ItemId;
    public int Price;
    public int IsLimited;
    public int LimitCount;
    public int Sort;
}