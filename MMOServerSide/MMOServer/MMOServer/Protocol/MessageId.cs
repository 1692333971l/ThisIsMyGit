namespace Protocol
{
    public enum MessageId
    {
        None = 0,

        // 登录请求响应
        LoginRequest = 2001,
        LoginResponse = 2002,
        // 注册请求响应
        RegisterRequest = 2003,
        RegisterResponse = 2004,
        // 角色创建
        CreateCharacterRequest = 3001,
        CreateCharacterResponse = 3002,
        // 获取角色列表
        GetCharacterListRequest = 3003,
        GetCharacterListResponse = 3004
    }
}