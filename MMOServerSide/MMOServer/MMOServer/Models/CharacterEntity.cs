namespace MMOServer.Models
{
    public class CharacterEntity
    {
        // 角色主键ID
        public int Id { get; set; }

        // 归属用户ID
        public int UserId { get; set; }

        // 角色名称
        public string Name { get; set; }

        // 职业
        public int Profession { get; set; }

        // 等级
        public int Level { get; set; }

        // 金币
        public int Gold { get; set; }

        // 基础属性
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }

        // 战斗属性
        public decimal CritRate { get; set; }
        public decimal CritDamage { get; set; }
        public int Defense { get; set; }

        // 当前/最大生命法力
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }

        // 地图与位置
        public int MapId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
    }
}