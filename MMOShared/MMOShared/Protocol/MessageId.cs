//请求响应id
namespace Protocol
{
    public enum MessageId
    {
        None = 0,

        // 登录请求响应
        LoginRequest = 1000,
        LoginResponse = 1001,
        // 注册请求响应
        RegisterRequest = 1002,
        RegisterResponse = 1003,
        // 角色创建
        CreateCharacterRequest = 1004,
        CreateCharacterResponse = 1005,
        // 获取角色列表
        GetCharacterListRequest = 1006,
        GetCharacterListResponse = 1007,
        // 进入主城
        EnterGameRequest = 1008,
        EnterGameResponse = 1009,
    }
}