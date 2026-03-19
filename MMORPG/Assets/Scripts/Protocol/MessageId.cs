namespace Protocol
{
    public enum MessageId
    {
        None = 0,

        // 登录请求
        LoginRequest = 2001,

        // 登录响应
        LoginResponse = 2002,

        // 注册请求
        RegisterRequest = 2003,

        // 注册响应
        RegisterResponse = 2004
    }
}