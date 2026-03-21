using System;

//登录请求
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