namespace Protocol
{
    [Serializable]
    public class GetCharacterListResponse
    {
        // 错误码
        public int ErrorCode;

        // 提示信息
        public string Message;

        // 当前用户拥有的角色列表
        public List<CharacterInfo> CharacterList;
    }
}