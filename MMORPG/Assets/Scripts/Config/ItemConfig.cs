using System;

//道具配置表结构
[Serializable]
public class ItemConfig
{
    public int ItemId;
    public string ItemName;
    public int ItemType;
    public int MaxStackCount;
    public int SellPrice;
    public int Quality;
    public string IconPath;
    public string Description;
    public int CanUse;
    public int UseEffectType;
    public int UseEffectValue;
}