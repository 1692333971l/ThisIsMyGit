using System;

//注册请求
namespace Protocol
{
    [Serializable]
    public class RegisterRequest
    {
        // 注册账号
        public string Account;

        // 注册密码
        public string Password;
    }
}