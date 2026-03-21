using System;

//职业配置表结构
[Serializable]
public class ProfessionConfig
{
    public int ProfessionId;
    public string ProfessionName;
    public string ModelPath;

    public int Strength;
    public int Agility;
    public int Intelligence;

    public float CritRate;
    public float CritDamage;
    public int Defense;

    public int Hp;
    public int Mp;
    public int MaxHp;
    public int MaxMp;

    public int MapId;
    public float PosX;
    public float PosY;
    public float PosZ;
}