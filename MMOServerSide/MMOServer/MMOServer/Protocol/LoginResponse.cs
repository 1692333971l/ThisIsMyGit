namespace Protocol
{
    [Serializable]
    public class LoginResponse
    {
        // 错误码：0表示成功，非0表示失败
        public int ErrorCode;

        // 给客户端显示的提示信息
        public string Message;

        // 登录成功后返回的用户Id
        public int UserId;

        // 登录成功后返回的账号名
        public string Account;
    }
}