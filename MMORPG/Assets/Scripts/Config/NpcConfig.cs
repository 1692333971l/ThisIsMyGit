using System;

//NPC而配置表结构
[Serializable]
public class NpcConfig
{
    public int NpcId;
    public string NpcName;
    public int HasTask;
    public int TaskId;
    public int HasShop;
    public int ShopId;
}