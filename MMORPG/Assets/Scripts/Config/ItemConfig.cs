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

    // 使用相关
    public int CanUse;
    public int UseEffectType;
    public int UseEffectValue;

    // 装备相关
    public int CanEquip;
    public int EquipSlotType;

    // 装备加成属性
    public int AddStrength;
    public int AddAgility;
    public int AddIntelligence;
    public int AddDefense;
    public int AddMaxHp;
    public int AddMaxMp;
    public int AddCritRate;
    public int AddCritDamage;
}