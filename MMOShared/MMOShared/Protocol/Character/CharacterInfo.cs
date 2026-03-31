using System;

//角色信息
namespace Protocol
{
    [Serializable]
    public class CharacterInfo
    {
        public int CharacterId;//角色ID
        public int UserId;//归属用户ID
        public string Name;//角色名称
        public int Profession;//职业
        public int Level;//等级
        public int Exp;//经验
        public int Gold;//金币
        public int Strength;//力量
        public int Agility;//敏捷
        public int Intelligence;//智慧
        public decimal CritRate;//暴击率
        public decimal CritDamage;//暴击伤害
        public int Defense;
        // 当前/最大生命法力
        public int Hp;
        public int Mp;
        public int MaxHp;
        public int MaxMp;
        // 地图与位置
        public int MapId;
        public float PosX;
        public float PosY;
        public float PosZ;
    }
}