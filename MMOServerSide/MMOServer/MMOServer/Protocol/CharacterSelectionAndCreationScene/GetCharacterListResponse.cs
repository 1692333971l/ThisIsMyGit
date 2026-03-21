namespace Protocol
{
    [Serializable]
    public class CreateCharacterResponse
    {
        // 错误码：0成功，非0失败
        public int ErrorCode;

        // 提示信息
        public string Message;

        // 创建成功后返回的新角色信息
        public CharacterInfo CharacterInfo;
    }
}