namespace Protocol
{
    [Serializable]
    public class RegisterResponse
    {
        // 错误码：0表示成功，非0表示失败
        public int ErrorCode;

        // 给客户端显示的提示信息
        public string Message;
    }
}