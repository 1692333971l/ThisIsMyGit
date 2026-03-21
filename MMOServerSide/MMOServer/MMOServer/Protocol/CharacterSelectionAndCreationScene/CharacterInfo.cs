namespace Protocol
{
    [Serializable]
    public class CharacterInfo
    {
        // 角色ID
        public int CharacterId;

        // 归属用户ID
        public int UserId;

        // 角色名称
        public string Name;

        // 职业
        public int Profession;

        // 等级
        public int Level;

        // 金币
        public int Gold;

        // 当前生命值
        public int Hp;

        // 当前法力值
        public int Mp;
    }
}