namespace Protocol
{
    [Serializable]
    public class CreateCharacterRequest
    {
        // 当前登录用户ID
        public int UserId;

        // 角色名称
        public string Name;

        // 选择的职业
        public int Profession;
    }
}