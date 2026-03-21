namespace Protocol
{
    [Serializable]
    public class LoginRequest
    {
        // 登录账号
        public string Account;

        // 登录密码
        public string Password;
    }
}